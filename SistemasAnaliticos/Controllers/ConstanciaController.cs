using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Helpers;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.ViewModels;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Auxiliares.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class ConstanciaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IConstanciaService _constanciaService;
        private readonly IPermisoAlcanceService _permisoAlcanceService;
        private readonly IEmailService _emailService;


        public ConstanciaController(DBContext context, UserManager<Usuario> userManager, IConstanciaService constanciaService, IPermisoAlcanceService permisoAlcanceService, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _constanciaService = constanciaService;
            _permisoAlcanceService = permisoAlcanceService;
            _emailService = emailService;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX
        [Authorize(Policy = "Constancia.Ver")]
        public async Task<IActionResult> VerConstancias(int page = 1)
        {
            int pageSize = 3;

            var query = _context.Constancia.AsNoTracking();
            query = await _permisoAlcanceService.AplicarAlcanceConstanciaAsync(query, User);

            var totalConstancias = await query.CountAsync();

            var constancias = await query
                .AsNoTracking()
                .OrderByDescending(x => x.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ConstanciaViewModel
                {
                    idConstancia = p.idConstancia,
                    fechaCreacion = p.fechaCreacion,
                    nombreEmpleado = p.nombreEmpleado,
                    departamento = p.departamento,
                    tipo = p.tipo,
                    dirijido = p.dirijido,
                    fechaRequerida = p.fechaRequerida,
                    comentarios = p.comentarios,
                    nombreArchivo = p.nombreArchivo,
                    tipoMIME = p.tipoMIME,
                    tamanoArchivo = p.tamanoArchivo,
                    estado = p.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionConstanciasViewModel
            {
                Constancias = constancias,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalConstancias / (double)pageSize)
            };

            return View(viewModel);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA TENER TODAS LAS CONSTANCIAS (sin paginación)
        public async Task<IActionResult> ObtenerTodasLasConstancias()
        {
            try
            {
                var query = _context.Constancia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceConstanciaAsync(query, User);

                var todosLasConstancias = await query
                    .AsNoTracking()
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idConstancia,
                        p.fechaCreacion,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.dirijido,
                        p.fechaRequerida,
                        p.comentarios,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo,
                        p.estado,
                    })
                    .ToListAsync();

                return Ok(todosLasConstancias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerConstanciasFiltradasCompletas(
            [FromQuery] string[] tipos,
            [FromQuery] string[] estados,
            [FromQuery] string[] departamentos,
            [FromQuery] string nombre = null,
            [FromQuery] string fechaTipo = null,
            [FromQuery] string fechaUnica = null,
            [FromQuery] string fechaDesde = null,
            [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.Constancia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceConstanciaAsync(query, User);

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipo));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    var estadosMapeados = estados.Select(e =>
                        e == "Creada" ? "Creada" :
                        e == "Aprobada" ? "Aprobada" :
                        e == "Pendiente" ? "Pendiente" :
                        "Rechazada").ToArray();

                    query = query.Where(p => estadosMapeados.Contains(p.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(p => departamentos.Contains(p.departamento));
                }

                // Aplicar filtro por nombre
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.nombreEmpleado) && p.nombreEmpleado.Contains(nombre));
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
                            query = query.Where(p =>
                                p.fechaCreacion >= desdeDate &&
                                p.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var contancias = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idConstancia,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.dirijido,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaRequerida,
                        p.comentarios,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo
                    })
                    .ToListAsync();

                return Ok(contancias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor al aplicar filtros");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR PERMISOS FILTRADOS EN EXCEL Y PDF
        public async Task<IActionResult> ExportarConstanciasExcel(
        [FromQuery] string[] tipos,
        [FromQuery] string[] estados,
        [FromQuery] string[] departamentos,
        [FromQuery] string nombre = null,
        [FromQuery] string fechaTipo = null,
        [FromQuery] string fechaUnica = null,
        [FromQuery] string fechaDesde = null,
        [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.Constancia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceConstanciaAsync(query, User);

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipo));
                }

                // Aplicar filtros de estado - SIN mapeo, directo
                if (estados != null && estados.Length > 0)
                {
                    query = query.Where(p => estados.Contains(p.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(p => departamentos.Contains(p.departamento));
                }

                // Aplicar filtro por nombre
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    query = query.Where(p => p.nombreEmpleado != null && p.nombreEmpleado.ToLower().Contains(nombre.ToLower()));
                }

                // Aplicar filtros de fecha
                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDT))
                    {
                        DateOnly fechaUnicaDate = DateOnly.FromDateTime(fechaUnicaDT);
                        query = query.Where(p => p.fechaCreacion == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(p =>
                                p.fechaCreacion >= desdeDate &&
                                p.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var contancias = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idConstancia,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.dirijido,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaRequerida,
                        p.comentarios,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo
                    })
                    .ToListAsync();

                // Crear el libro de Excel
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Contancias");

                // Estilos para el encabezado
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Encabezados
                worksheet.Cell(1, 1).Value = "ID Constancia";
                worksheet.Cell(1, 2).Value = "Nombre Empleado";
                worksheet.Cell(1, 3).Value = "Departamento";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Estado";
                worksheet.Cell(1, 6).Value = "Fecha Creación";
                worksheet.Cell(1, 7).Value = "Fecha Requerida";
                worksheet.Cell(1, 8).Value = "Dirijido a";
                worksheet.Cell(1, 9).Value = "Comentarios";
                worksheet.Cell(1, 10).Value = "Archivo Adjunto";

                // Aplicar estilo al encabezado
                worksheet.Range(1, 1, 1, 13).Style = headerStyle;

                // Llenar datos
                int row = 2;
                foreach (var permiso in contancias)
                {
                    worksheet.Cell(row, 1).Value = permiso.idConstancia;
                    worksheet.Cell(row, 2).Value = permiso.nombreEmpleado;
                    worksheet.Cell(row, 3).Value = permiso.departamento;
                    worksheet.Cell(row, 4).Value = permiso.tipo;
                    worksheet.Cell(row, 5).Value = permiso.estado; // Estado directo, sin mapeo
                    worksheet.Cell(row, 6).Value = permiso.fechaCreacion.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 7).Value = permiso.fechaRequerida?.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 8).Value = permiso.dirijido;
                    worksheet.Cell(row, 9).Value = permiso.comentarios;
                    worksheet.Cell(row, 9).Value = permiso.comentarios;
                    worksheet.Cell(row, 10).Value = string.IsNullOrEmpty(permiso.nombreArchivo) ? "No" : "Sí";

                    row++;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                // Preparar respuesta
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Constancias_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await _userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Constancia",
                    Accion = "Exportación Excel"
                };
                _context.Auditoria.Add(auditoria);

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el archivo Excel: {ex.Message}");
            }
        }

        public async Task<IActionResult> ExportarConstanciasPDF(
        [FromQuery] string[] tipos,
        [FromQuery] string[] estados,
        [FromQuery] string[] departamentos,
        [FromQuery] string nombre = null,
        [FromQuery] string fechaTipo = null,
        [FromQuery] string fechaUnica = null,
        [FromQuery] string fechaDesde = null,
        [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.Constancia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceConstanciaAsync(query, User);

                // Aplicar filtros (misma lógica que Excel)
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipo));
                }

                if (estados != null && estados.Length > 0)
                {
                    query = query.Where(p => estados.Contains(p.estado));
                }

                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(p => departamentos.Contains(p.departamento));
                }

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    query = query.Where(p => p.nombreEmpleado != null && p.nombreEmpleado.ToLower().Contains(nombre.ToLower()));
                }

                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDT))
                    {
                        DateOnly fechaUnicaDate = DateOnly.FromDateTime(fechaUnicaDT);
                        query = query.Where(p => p.fechaCreacion == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(p =>
                                p.fechaCreacion >= desdeDate &&
                                p.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var contancias = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idConstancia,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.dirijido,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaRequerida,
                        p.comentarios,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo
                    })
                    .ToListAsync();

                // Crear PDF
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Título - CORREGIDO
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var titulo = new Paragraph("REPORTE DE CONSTANCIAS", tituloFont);
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
                AddTableCell(table, "Empleado", headerFont);
                AddTableCell(table, "Departamento", headerFont);
                AddTableCell(table, "Tipo", headerFont);
                AddTableCell(table, "Estado", headerFont);
                AddTableCell(table, "Fechas", headerFont);
                AddTableCell(table, "Dirijido a", headerFont);
                AddTableCell(table, "Comentarios", headerFont);

                // Datos - CORREGIDO
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                foreach (var contancia in contancias)
                {
                    AddTableCell(table, contancia.idConstancia.ToString(), cellFontBold);
                    AddTableCell(table, contancia.nombreEmpleado ?? "", cellFont);
                    AddTableCell(table, contancia.departamento ?? "", cellFont);
                    AddTableCell(table, contancia.tipo ?? "", cellFont);

                    // Color según estado
                    var estadoFont = GetEstadoFont(contancia.estado);
                    AddTableCell(table, contancia.estado ?? "", estadoFont);

                    var fechas = $"Creación: {contancia.fechaCreacion:dd/MM/yyyy}\nRequerida: {contancia.fechaRequerida:dd/MM/yyyy}";
                    AddTableCell(table, fechas, cellFont);

                    AddTableCell(table, contancia.dirijido ?? "Sin dirijir", cellFont);
                    AddTableCell(table, contancia.comentarios ?? "Sin comentarios", cellFont);
                }

                document.Add(table);

                // Pie de página - CORREGIDO
                var pieFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC);
                var pie = new Paragraph($"Total de registros: {contancias.Count}", pieFont);
                pie.Alignment = Element.ALIGN_RIGHT;
                pie.SpacingBefore = 10f;
                document.Add(pie);

                document.Close();

                // Devolver PDF
                var fileName = $"Constancias_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await _userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Constancia",
                    Accion = "Exportación PDF"
                };
                _context.Auditoria.Add(auditoria);

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
                var query = _context.Constancia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceConstanciaAsync(query, User);

                var contadores = await query
                    .AsNoTracking()
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        // Contadores por tipo
                        Laboral = g.Count(p => p.tipo == "Laboral"),
                        Salarial = g.Count(p => p.tipo == "Salarial"),

                        // Contadores por estado
                        Creada = g.Count(p => p.estado == "Creada"),
                        Aprobada = g.Count(p => p.estado == "Aprobada"),
                        Pendiente = g.Count(p => p.estado == "Pendiente"),
                        Rechazada = g.Count(p => p.estado == "Rechazada"),

                        // Contadores por departamento ← AGREGAR ESTOS
                        Bodega = g.Count(p => p.departamento == "Bodega"),
                        FinancieroContable = g.Count(p => p.departamento == "Financiero Contable"),
                        Gerencia = g.Count(p => p.departamento == "Gerencia"),
                        Ingenieria = g.Count(p => p.departamento == "Ingeniería"),
                        Jefatura = g.Count(p => p.departamento == "Jefatura"),
                        Legal = g.Count(p => p.departamento == "Legal"),
                        Operaciones = g.Count(p => p.departamento == "Operaciones"),
                        RecursosHumanos = g.Count(p => p.departamento == "Recursos Humanos"),
                        TecnicosNCR = g.Count(p => p.departamento == "Tecnicos NCR"),
                        TecnologiasInformacion = g.Count(p => p.departamento == "Tecnologías de Información"),
                        Ventas = g.Count(p => p.departamento == "Ventas")
                    })
                    .FirstOrDefaultAsync();

                return Ok(contadores ?? new
                {
                    Laboral = 0,
                    Salarial = 0,
                    Creada = 0,
                    Aprobada = 0,
                    Pendiente = 0,
                    Rechazada = 0,
                    Bodega = 0,
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    RecursosHumanos = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Laboral = 0,
                    Salarial = 0,
                    Creada = 0,
                    Aprobada = 0,
                    Pendiente = 0,
                    Rechazada = 0,
                    Bodega = 0,
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    RecursosHumanos = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0
                });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // VER DETALLES DE PERMISOS
        [Authorize(Policy = "Constancia.Detalles")]
        [HttpGet]
        [Route("Constancia/Details/{id}")]
        public async Task<ActionResult> Details(long id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de constancia válido.";
                return RedirectToAction("VerConstancias", "Constancia");
            }

            var constancia = await _context.Constancia.FindAsync(id);

            if (constancia == null)
            {
                TempData["ErrorMessage"] = "La constancia que intentas editar no existe o fue eliminado.";
                return RedirectToAction("VerConstancias", "Constancia");
            }

            return View(constancia);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR ADJUNTO
        [Authorize(Policy = "Constancia.Descargar")]
        [HttpGet]
        [Route("Constancia/descargar-adjunto/{id}")]
        public async Task<IActionResult> DescargarAdjunto(long id)
        {
            var constancia = await _context.Constancia
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.idConstancia == id);

            if (constancia?.datosAdjuntos == null || constancia.datosAdjuntos.Length == 0)
            {
                return NotFound("No se encontró el archivo adjunto");
            }

            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
            var usuario = await _userManager.GetUserAsync(User);

            // Auditoría
            var auditoria = new Auditoria
            {
                Fecha = hoy,
                Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                Usuario = usuario.nombreCompleto ?? "Desconocido",
                Tabla = "Constancia",
                Accion = "Descarga de Adjunto"
            };
            _context.Auditoria.Add(auditoria);

            // Retornar el archivo
            return File(
                constancia.datosAdjuntos,
                constancia.tipoMIME ?? "application/octet-stream",
                constancia.nombreArchivo ?? "archivo_adjunto"
            );
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // PREVIUEW ADJUNTO
        [Authorize(Policy = "Constancia.Detalles")]
        [HttpGet]
        [Route("Constancia/Preview/{id}")]
        public async Task<IActionResult> Preview(long id)
        {
            var constancia = await _context.Constancia.FindAsync(id);

            if (constancia == null || constancia.datosAdjuntos == null)
                return NotFound();

            return File(
                constancia.datosAdjuntos,
                constancia.tipoMIME,
                enableRangeProcessing: true
            );
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CAMBIOS DE ESTADOS MASIVOS
        [Authorize(Policy = "Constancia.CambiarEstado")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMasivo([FromBody] EstadoMasivoViewModel model)
        {
            foreach (var id in model.Ids)
            {
                var constancia = await _context.Constancia.FindAsync(id);
                if (constancia != null)
                {
                    constancia.estado = model.estado;
                }
                var usuarioPermiso = await _userManager.FindByIdAsync(constancia.UsuarioId);
                try
                {
                    if (constancia.estado == "Aprobada")
                    {
                        // 1. Correo Aprobado
                        var htmlEmpleado = PlantillasEmail.EstadoEmpleadoAprob(
                            nombreEmpleado: usuarioPermiso.nombreCompleto,
                            tipoPermiso: constancia.tipo
                        );

                        await _emailService.SendEmailAsync(
                            toEmail: usuarioPermiso.Email,
                            toName: usuarioPermiso.nombreCompleto,
                            subject: $"Aprobación de Constancia - {usuarioPermiso.nombreCompleto}",
                            htmlBody: htmlEmpleado
                        );
                    }
                    else if (constancia.estado == "Rechazada")
                    {
                        // 2. Correo Rechazado
                        var htmlEmpleado = PlantillasEmail.EstadoEmpleadoRechaz(
                            nombreEmpleado: usuarioPermiso.nombreCompleto,
                            tipoPermiso: constancia.tipo
                        );
                        await _emailService.SendEmailAsync(
                            toEmail: usuarioPermiso.Email,
                            toName: usuarioPermiso.nombreCompleto,
                            subject: $"Rechazo de Constancia - {usuarioPermiso.nombreCompleto}",
                            htmlBody: htmlEmpleado
                        );
                    }
                }
                catch (Exception exEmail)
                {
                    // Solo log si hay error en correo, no interrumpir
                    Console.WriteLine($"Error enviando correo: {exEmail.Message}");
                }
            }

            await _context.SaveChangesAsync();

            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
            var usuario = await _userManager.GetUserAsync(User);

            // Auditoría
            var auditoria = new Auditoria
            {
                Fecha = hoy,
                Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                Usuario = usuario.nombreCompleto ?? "Desconocido",
                Tabla = "Constancia",
                Accion = "Cambio de Estado del no." +string.Join(", ", model.Ids)
            };
            _context.Auditoria.Add(auditoria);

            TempData["SuccessMessageCons"] = "Se cambiaron las constancias de estados correctamente.";
            return Ok(new { redirect = Url.Action("VerConstancias") });
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // HACER REGISTROS DE CONSTANCIAS
        [Authorize(Policy = "Constancia.Crear")]
        public async Task<IActionResult> Create(Constancia model)
        {
            var usuario = await _userManager.GetUserAsync(User);

            string nombreJefe = "No asignado";
            string correoJefe = "No asignado";

            if (!string.IsNullOrEmpty(usuario.jefeId))
            {
                var jefe = await _userManager.FindByIdAsync(usuario.jefeId);
                if (jefe != null)
                {
                    nombreJefe = jefe.nombreCompleto ?? jefe.UserName;
                    correoJefe = jefe.Email;
                }
            }

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

                if (model.tipo == "Laboral")
                {
                    var pdfBytes = await _constanciaService.GenerarConstanciaLaboral(usuario.nombreCompleto, usuario.cedula, usuario.departamento, usuario.fechaIngreso, usuario.puesto, model.dirijido);

                    var nuevo = new Constancia
                    {
                        UsuarioId = usuario.Id,

                        fechaCreacion = hoy,
                        nombreEmpleado = usuario.nombreCompleto,
                        departamento = usuario.departamento,
                        tipo = model.tipo,

                        dirijido = model.dirijido,
                        fechaRequerida = model.fechaRequerida,
                        comentarios = model.comentarios,

                        datosAdjuntos = pdfBytes,
                        nombreArchivo = $"Constancia_Laboral_{usuario.primerNombre}{usuario.primerApellido}.pdf",
                        tipoMIME = "application/pdf",
                        tamanoArchivo = pdfBytes.Length,

                        estado = "Creada"
                    };

                    _context.Constancia.Add(nuevo);

                    // Auditoría
                    var auditoria = new Auditoria
                    {
                        Fecha = hoy,
                        Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                        Usuario = usuario.nombreCompleto ?? "Desconocido",
                        Tabla = "Constancia",
                        Accion = "Nuevo registro"
                    };
                    _context.Auditoria.Add(auditoria);

                    await _context.SaveChangesAsync();
                } else
                {
                    var nuevo = new Constancia
                    {
                        fechaCreacion = hoy,
                        nombreEmpleado = usuario.nombreCompleto,
                        departamento = usuario.departamento,
                        tipo = model.tipo,

                        dirijido = "sin dirijido",
                        fechaRequerida = model.fechaRequerida,
                        comentarios = model.comentarios,

                        estado = "Creada"
                    };

                    _context.Constancia.Add(nuevo);

                    // Auditoría
                    var auditoria = new Auditoria
                    {
                        Fecha = hoy,
                        Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                        Usuario = usuario.nombreCompleto ?? "Desconocido",
                        Tabla = "Constancia",
                        Accion = "Nuevo registro"
                    };
                    _context.Auditoria.Add(auditoria);

                    await _context.SaveChangesAsync();
                }

                try
                {
                    // 1. Correo al empleado
                    var htmlEmpleado = PlantillasEmail.ConfirmacionEmpleadoCons(
                        nombreEmpleado: usuario.nombreCompleto,
                        tipoConstancia: model.tipo
                    );

                    await _emailService.SendEmailAsync(
                        toEmail: usuario.Email,
                        toName: usuario.nombreCompleto,
                        subject: $"Confirmación de Constancia - {usuario.nombreCompleto}",
                        htmlBody: htmlEmpleado
                    );

                    //2.Correo de notificación al jefe(si tiene)
                    if (!string.IsNullOrEmpty(usuario.jefeId))
                    {
                        var htmlJefe = PlantillasEmail.NotificacionJefaturaCons(
                            nombreEmpleado: usuario.nombreCompleto,
                            tipoConstancia: model.tipo,
                            nombreJefe: nombreJefe
                        );

                        await _emailService.SendEmailAsync(
                            toEmail: correoJefe,
                            toName: nombreJefe,
                            subject: $"Solicitud de constancia pendiente - {usuario.nombreCompleto}",
                            htmlBody: htmlJefe
                        );
                    }

                }
                catch (Exception exEmail)
                {
                    // Solo log si hay error en correo, no interrumpir
                    Console.WriteLine($"Error enviando correo: {exEmail.Message}");
                }

                TempData["SuccessMessage"] = "Se creó la constancia correctamente.";
                return RedirectToAction("Index", "Permiso");
            }
            catch
            {
                TempData["ErrorMessage"] = "Error en la creación de la constancia.";
                return RedirectToAction("Index", "Permiso");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CREAR DOCUMENTO DE CONSTANCIA SALARIAL
        [Authorize(Policy = "Constancia.Crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HacerConstanciaSalarial(long idConstancia, int salarioBruto, int deducciones, int salarioNeto)
        {
            if (idConstancia == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de empleado válido.";
                return RedirectToAction("Index", "Usuario");
            }

            var constancia = await _context.Constancia.FindAsync(idConstancia);
            if (constancia == null)
            {
                TempData["ErrorMessage"] = "El empleado que intentas editar no existe o fue eliminado.";
                return RedirectToAction("Index", "Usuario");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var pdfBytes = await _constanciaService.GenerarConstanciaSalarial(user.nombreCompleto, user.cedula, user.departamento, user.fechaIngreso, user.puesto, salarioBruto, deducciones, salarioNeto);

                // 🔹 Actualizar campos personales
                constancia.datosAdjuntos = pdfBytes;
                constancia.nombreArchivo = $"Constancia_Salarial_{user.primerNombre}{user.primerApellido}.pdf";
                constancia.tipoMIME = "application/pdf";
                constancia.tamanoArchivo = pdfBytes.Length;

                // 🔹 Guardar cambios
                _context.Update(constancia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageCons"] = "Se creó correctamente la constancia salarial.";
                return RedirectToAction("VerConstancias", "Constancia");
            }
            catch (Exception ex)
            {
                // 🧩 Manejo de error
                TempData["ErrorMessageCons"] = "Ocurrió un error al crear la constancia: " + ex.Message;
                return RedirectToAction("VerConstancias", "Constancia");
            }
        }
    }
}