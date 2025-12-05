using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Models.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class BeneficioController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;


        public BeneficioController(DBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX
        public async Task<IActionResult> VerBeneficios(int page = 1)
        {
            int pageSize = 3;

            var totalBeneficios = await _context.Beneficio
                .CountAsync();

            var beneficios = await _context.Beneficio
                .AsNoTracking()
                .OrderByDescending(x => x.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BeneficioViewModel
                {
                    idBeneficio = b.idBeneficio,
                    fechaCreacion = b.fechaCreacion,
                    nombreEmpleado = b.nombreEmpleado,
                    departamento = b.departamento,
                    tipo = b.tipo,
                    monto = b.monto,
                    comentarios = b.comentarios,
                    estado = b.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionBeneficiosViewModel
            {
                Beneficios = beneficios,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalBeneficios / (double)pageSize)
            };

            return View(viewModel);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA TENER TODOS LOS BENEFICIOS (sin paginación)
        public async Task<IActionResult> ObtenerTodosLosBeneficios()
        {
            try
            {
                var todosLosBeneficios = await _context.Beneficio
                    .AsNoTracking()
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(b => new {
                        b.idBeneficio,
                        b.fechaCreacion,
                        b.nombreEmpleado,
                        b.departamento,
                        b.tipo,
                        b.monto,
                        b.comentarios,
                        b.estado,
                    })
                    .ToListAsync();

                return Ok(todosLosBeneficios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerBeneficiosFiltradosCompletos(
        [FromQuery] string[] tipos,
        [FromQuery] string[] estados,
        [FromQuery] string[] departamentos,
        [FromQuery] string fechaTipo = null,
        [FromQuery] string fechaUnica = null,
        [FromQuery] string fechaDesde = null,
        [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.Beneficio.AsNoTracking().AsQueryable();

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(b => tipos.Contains(b.tipo));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobado" ? "Creada" :
                        e == "Pendiente" ? "Pendiente" :
                        "Rechazada").ToArray();

                    query = query.Where(b => estadosMapeados.Contains(b.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(b => departamentos.Contains(b.departamento));
                }

                // Aplicar filtros de fecha
                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (fechaTipo == "single" && !string.IsNullOrEmpty(fechaUnica))
                    {
                        if (DateOnly.TryParse(fechaUnica, out DateOnly fechaUnicaDate))
                        {
                            query = query.Where(p => p.fechaCreacion == fechaUnicaDate);
                        }
                    }

                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(b =>
                                b.fechaCreacion >= desdeDate &&
                                b.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var beneficios = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(b => new {
                        b.idBeneficio,
                        b.fechaCreacion,
                        b.nombreEmpleado,
                        b.departamento,
                        b.tipo,
                        b.monto,
                        b.comentarios,
                        b.estado,
                    })
                    .ToListAsync();

                return Ok(beneficios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor al aplicar filtros");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR PERMISOS FILTRADOS EN EXCEL Y PDF
        public async Task<IActionResult> ExportarBeneficiosExcel(
        [FromQuery] string[] tipos,
        [FromQuery] string[] estados,
        [FromQuery] string[] departamentos,
        [FromQuery] string fechaTipo = null,
        [FromQuery] string fechaUnica = null,
        [FromQuery] string fechaDesde = null,
        [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.Beneficio.AsNoTracking().AsQueryable();

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(b => tipos.Contains(b.tipo));
                }

                // Aplicar filtros de estado - SIN mapeo, directo
                if (estados != null && estados.Length > 0)
                {
                    query = query.Where(b => estados.Contains(b.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(b => departamentos.Contains(b.departamento));
                }

                // Aplicar filtros de fecha
                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDT))
                    {
                        DateOnly fechaUnicaDate = DateOnly.FromDateTime(fechaUnicaDT);
                        query = query.Where(b => b.fechaCreacion == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(b =>
                                b.fechaCreacion >= desdeDate &&
                                b.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var beneficios = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idBeneficio,
                        p.fechaCreacion,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.monto,
                        p.comentarios,
                        p.estado
                    })
                    .ToListAsync();

                // Crear el libro de Excel
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Beneficios");

                // Estilos para el encabezado
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Encabezados
                worksheet.Cell(1, 1).Value = "ID Beneficio";
                worksheet.Cell(1, 2).Value = "Fecha Creación";
                worksheet.Cell(1, 3).Value = "Nombre Empleado";
                worksheet.Cell(1, 4).Value = "Departamento";
                worksheet.Cell(1, 5).Value = "Tipo";
                worksheet.Cell(1, 6).Value = "Monto";
                worksheet.Cell(1, 7).Value = "Estado";
                worksheet.Cell(1, 8).Value = "Comentarios";

                // Aplicar estilo al encabezado
                worksheet.Range(1, 1, 1, 8).Style = headerStyle;

                // Llenar datos
                int row = 2;
                foreach (var permiso in beneficios)
                {
                    worksheet.Cell(row, 1).Value = permiso.idBeneficio;
                    worksheet.Cell(row, 2).Value = permiso.fechaCreacion.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 3).Value = permiso.nombreEmpleado;
                    worksheet.Cell(row, 4).Value = permiso.departamento;
                    worksheet.Cell(row, 5).Value = permiso.tipo;
                    worksheet.Cell(row, 6).Value = permiso.monto;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(row, 7).Value = permiso.estado; 
                    worksheet.Cell(row, 8).Value = permiso.comentarios;
                    row++;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                // Preparar respuesta
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Beneficios_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el archivo Excel: {ex.Message}");
            }
        }

        public async Task<IActionResult> ExportarBeneficiosPDF(
        [FromQuery] string[] tipos,
        [FromQuery] string[] estados,
        [FromQuery] string[] departamentos,
        [FromQuery] string fechaTipo = null,
        [FromQuery] string fechaUnica = null,
        [FromQuery] string fechaDesde = null,
        [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.Beneficio.AsNoTracking().AsQueryable();

                // Aplicar filtros (misma lógica que Excel)
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(b => tipos.Contains(b.tipo));
                }

                if (estados != null && estados.Length > 0)
                {
                    query = query.Where(b => estados.Contains(b.estado));
                }

                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(b => departamentos.Contains(b.departamento));
                }

                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDT))
                    {
                        DateOnly fechaUnicaDate = DateOnly.FromDateTime(fechaUnicaDT);
                        query = query.Where(b => b.fechaCreacion == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(b =>
                                b.fechaCreacion >= desdeDate &&
                                b.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var beneficios = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(b => new {
                        b.idBeneficio,
                        b.fechaCreacion,
                        b.nombreEmpleado,
                        b.departamento,
                        b.tipo,
                        b.monto,
                        b.comentarios,
                        b.estado
                    })
                    .ToListAsync();

                // Crear PDF
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Título - CORREGIDO
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var titulo = new Paragraph("REPORTE DE BENEFICIOS", tituloFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 20f;
                document.Add(titulo);

                // Fecha de generación - CORREGIDO
                var fechaFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC);
                var fechaGeneracion = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fechaFont);
                fechaGeneracion.Alignment = Element.ALIGN_RIGHT;
                fechaGeneracion.SpacingAfter = 10f;
                document.Add(fechaGeneracion);

                // Crear tabla
                var table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.8f, 1.5f, 1.5f, 1.2f, 1.2f, 1.5f, 2f, 2f });

                // Encabezados de tabla - CORREGIDO
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                AddTableCell(table, "ID", headerFont);
                AddTableCell(table, "Fecha Creación", headerFont);
                AddTableCell(table, "Empleado", headerFont);
                AddTableCell(table, "Departamento", headerFont);
                AddTableCell(table, "Tipo", headerFont);
                AddTableCell(table, "Monto", headerFont);
                AddTableCell(table, "Comentarios", headerFont);
                AddTableCell(table, "Estado", headerFont);

                // Datos - CORREGIDO
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                foreach (var beneficio in beneficios)
                {
                    AddTableCell(table, beneficio.idBeneficio.ToString(), cellFontBold);
                    AddTableCell(table, beneficio.fechaCreacion.ToString("dd/MM/yyyy"), cellFont);
                    AddTableCell(table, beneficio.nombreEmpleado ?? "", cellFont);
                    AddTableCell(table, beneficio.departamento ?? "", cellFont);
                    AddTableCell(table, beneficio.tipo ?? "", cellFont);
                    AddTableCell(table, beneficio.monto.ToString("#,##0") ?? "Sin monto", cellFont);
                    AddTableCell(table, beneficio.comentarios ?? "Sin comentarios", cellFont);

                    // Color según estado
                    var estadoFont = GetEstadoFont(beneficio.estado);
                    AddTableCell(table, beneficio.estado ?? "", estadoFont);
                }

                document.Add(table);

                // Pie de página - CORREGIDO
                var pieFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC);
                var pie = new Paragraph($"Total de registros: {beneficios.Count}", pieFont);
                pie.Alignment = Element.ALIGN_RIGHT;
                pie.SpacingBefore = 10f;
                document.Add(pie);

                document.Close();

                // Devolver PDF
                var fileName = $"Beneficios_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(memoryStream.ToArray(), "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
            }
        }

        // Métodos auxiliares - CORREGIDOS
        private void AddTableCell(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 5;
            cell.BorderWidth = 0.5f;
            table.AddCell(cell);
        }

        private Font GetEstadoFont(string estado)
        {
            var baseFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            if (string.IsNullOrEmpty(estado)) return baseFont;

            return estado.ToUpper() switch
            {
                "APROBADO" or "CREADA" or "APROBADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Green),
                "PENDIENTE" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Orange),
                "RECHAZADO" or "RECHAZADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Red),
                _ => baseFont
            };
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA OBTENER CONTADORES DE FILTROS
        public async Task<IActionResult> ObtenerContadoresFiltros()
        {
            try
            {
                var contadores = await _context.Beneficio
                    .AsNoTracking()
                    .GroupBy(b => 1)
                    .Select(g => new
                    {
                        // Contadores bor tipo
                        PrestamoLaboral = g.Count(b => b.tipo == "Préstamo Laboral"),
                        AdelantoSalarial = g.Count(b => b.tipo == "Adelanto Salarial"),

                        // Contadores por estado
                        Aprobado = g.Count(b => b.estado == "Creada" || b.estado == "Aprobada"),
                        Pendiente = g.Count(b => b.estado == "Pendiente"),
                        Rechazado = g.Count(b => b.estado == "Rechazada"),

                        // Contadores por departamento ← AGREGAR ESTOS
                        FinancieroContable = g.Count(b => b.departamento == "Financiero Contable"),
                        Gerencia = g.Count(b => b.departamento == "Gerencia"),
                        Ingenieria = g.Count(b => b.departamento == "Ingeniería"),
                        Jefatura = g.Count(b => b.departamento == "Jefatura"),
                        Legal = g.Count(b => b.departamento == "Legal"),
                        Operaciones = g.Count(b => b.departamento == "Operaciones"),
                        TecnicosNCR = g.Count(b => b.departamento == "Tecnicos NCR"),
                        TecnologiasInformacion = g.Count(b => b.departamento == "Tecnologías de Información"),
                        Ventas = g.Count(b => b.departamento == "Ventas")
                    })
                    .FirstOrDefaultAsync();

                return Ok(contadores ?? new
                {
                    PrestamoLaboral = 0,
                    AdelantoSalarial = 0,
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazado = 0,
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    PrestamoLaboral = 0,
                    AdelantoSalarial = 0,
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazado = 0,
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0
                });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // VER DETALLES DE PERMISOS
        [HttpGet]
        [Route("Beneficio/Details/{id}")]
        public async Task<ActionResult> Details(long id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de beneficio válido.";
                return RedirectToAction("VerBeneficios", "Beneficio");
            }

            var beneficio = await _context.Beneficio.FindAsync(id);

            if (beneficio == null)
            {
                TempData["ErrorMessage"] = "El beneficio que intentas editar no existe o fue eliminado.";
                return RedirectToAction("VerBeneficios", "Beneficio");
            }

            return View(beneficio);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // HACER REGISTROS DE BENEFICIOS
        public async Task<IActionResult> Create(Beneficio model)
        {
            // Detectar sistema operativo y usar el ID de zona horaria adecuado
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);

            try
            {
                var fotoService = new CodigoFotos();
                var adjuntoService = new ProcesarAdjuntos();
                var user = await _userManager.GetUserAsync(User);

                var nuevo = new Beneficio
                {
                    fechaCreacion = hoy,
                    nombreEmpleado = user.nombreCompleto,
                    departamento = user.departamento,
                    tipo = model.tipo,

                    monto = model.monto,
                    comentarios = model.comentarios,

                    estado = "Creada"
                };

                _context.Beneficio.Add(nuevo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Se creó el beneficio correctamente.";
                return RedirectToAction("Index", "Permiso");
            }
            catch
            {
                TempData["ErrorMessage"] = "Error en la creación del beneficio.";
                return RedirectToAction("Index", "Permiso");
            }
        }

    }
}
