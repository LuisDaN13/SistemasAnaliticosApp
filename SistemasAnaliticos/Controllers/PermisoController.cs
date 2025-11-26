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
    public class PermisoController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;

        public PermisoController(DBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX DONDE SE OCUPAN CREAR PERMISOS
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX DONDE SE VEN LOS PERMISOS
        public async Task<IActionResult> VerPermisos(int page = 1)
        {
            int pageSize = 3;

            var totalPermisos = await _context.Permiso
                .Where(p => p.estado == "Creada")
                .CountAsync();

            var permisos = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.estado == "Creada")
                .OrderByDescending(x => x.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PermisoViewModel
                {
                    idPermiso = p.idPermiso,
                    fechaCreacion = p.fechaCreacion,
                    nombreEmpleado = p.nombreEmpleado,
                    departamento = p.departamento,
                    tipo = p.tipo,
                    fechaInicio = p.fechaInicio,
                    fechaFinalizacion = p.fechaFinalizacion,
                    fechaRegresoLaboral = p.fechaRegresoLaboral,
                    horaCita = p.horaCita,
                    motivo = p.motivo,
                    comentarios = p.comentarios,
                    foto = p.foto,
                    nombreArchivo = p.nombreArchivo,
                    tipoMIME = p.tipoMIME,
                    tamanoArchivo = p.tamanoArchivo,
                    estado = p.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionPermisosViewModel
            {
                Permisos = permisos,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalPermisos / (double)pageSize)
            };

            return View(viewModel);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA TENER TODOS LO PERMISOS (sin paginación)
        public async Task<IActionResult> ObtenerTodosLosPermisos()
        {
            try
            {
                var todosLosPermisos = await _context.Permiso
                    .AsNoTracking()
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idPermiso,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaInicio,
                        p.fechaFinalizacion,
                        p.fechaRegresoLaboral,
                        p.horaCita,
                        p.motivo,
                        p.comentarios,
                        p.foto,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo
                    })
                    .ToListAsync();

                return Ok(todosLosPermisos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerPermisosFiltradosCompletos(
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
                var query = _context.Permiso.AsNoTracking().AsQueryable();

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipo));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobado" ? "Creada" :
                        e == "Pendiente" ? "Pendiente" :
                        "Rechazada").ToArray();

                    query = query.Where(p => estadosMapeados.Contains(p.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(p => departamentos.Contains(p.departamento));
                }

                // Aplicar filtros de fecha
                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (fechaTipo == "single" && !string.IsNullOrEmpty(fechaUnica))
                    {
                        if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDate))
                        {
                            query = query.Where(p =>
                                p.fechaCreacion.Date == fechaUnicaDate.Date ||
                                p.fechaCreacion.Date == fechaUnicaDate.Date);
                        }
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateTime.TryParse(fechaDesde, out DateTime desdeDate) &&
                            DateTime.TryParse(fechaHasta, out DateTime hastaDate))
                        {
                            query = query.Where(p =>
                                (p.fechaCreacion.Date >= desdeDate.Date && p.fechaCreacion.Date <= hastaDate.Date) ||
                                (p.fechaCreacion.Date >= desdeDate.Date && p.fechaCreacion.Date <= hastaDate.Date));
                        }
                    }
                }

                var permisos = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new {
                        p.idPermiso,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaInicio,
                        p.fechaFinalizacion,
                        p.fechaRegresoLaboral,
                        p.horaCita,
                        p.motivo,
                        p.comentarios,
                        p.foto,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo
                    })
                    .ToListAsync();

                return Ok(permisos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor al aplicar filtros");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR PERMISOS FILTRADOS EN EXCEL Y PDF
        public async Task<IActionResult> ExportarPermisosExcel(
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
                var query = _context.Permiso.AsNoTracking().AsQueryable();

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

                // Aplicar filtros de fecha
                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (fechaTipo == "single" && !string.IsNullOrEmpty(fechaUnica))
                    {
                        if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDate))
                        {
                            query = query.Where(p =>
                                p.fechaCreacion.Date == fechaUnicaDate.Date ||
                                p.fechaCreacion.Date == fechaUnicaDate.Date);
                        }
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateTime.TryParse(fechaDesde, out DateTime desdeDate) &&
                            DateTime.TryParse(fechaHasta, out DateTime hastaDate))
                        {
                            query = query.Where(p =>
                                (p.fechaCreacion.Date >= desdeDate.Date && p.fechaCreacion.Date <= hastaDate.Date) ||
                                (p.fechaCreacion.Date >= desdeDate.Date && p.fechaCreacion.Date <= hastaDate.Date));
                        }
                    }
                }

                var permisos = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new
                    {
                        p.idPermiso,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaInicio,
                        p.fechaFinalizacion,
                        p.fechaRegresoLaboral,
                        p.horaCita,
                        p.motivo,
                        p.comentarios,
                        p.nombreArchivo
                    })
                    .ToListAsync();

                // Crear el libro de Excel
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Permisos");

                // Estilos para el encabezado
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Encabezados
                worksheet.Cell(1, 1).Value = "ID Permiso";
                worksheet.Cell(1, 2).Value = "Nombre Empleado";
                worksheet.Cell(1, 3).Value = "Departamento";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Estado";
                worksheet.Cell(1, 6).Value = "Fecha Creación";
                worksheet.Cell(1, 7).Value = "Fecha Inicio";
                worksheet.Cell(1, 8).Value = "Fecha Finalización";
                worksheet.Cell(1, 9).Value = "Fecha Regreso Laboral";
                worksheet.Cell(1, 10).Value = "Hora Cita";
                worksheet.Cell(1, 11).Value = "Motivo";
                worksheet.Cell(1, 12).Value = "Comentarios";
                worksheet.Cell(1, 13).Value = "Archivo Adjunto";

                // Aplicar estilo al encabezado
                worksheet.Range(1, 1, 1, 13).Style = headerStyle;

                // Llenar datos
                int row = 2;
                foreach (var permiso in permisos)
                {
                    worksheet.Cell(row, 1).Value = permiso.idPermiso;
                    worksheet.Cell(row, 2).Value = permiso.nombreEmpleado;
                    worksheet.Cell(row, 3).Value = permiso.departamento;
                    worksheet.Cell(row, 4).Value = permiso.tipo;
                    worksheet.Cell(row, 5).Value = permiso.estado; // Estado directo, sin mapeo
                    worksheet.Cell(row, 6).Value = permiso.fechaCreacion.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 7).Value = permiso.fechaInicio?.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 8).Value = permiso.fechaFinalizacion?.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 9).Value = permiso.fechaRegresoLaboral?.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 10).Value = permiso.horaCita?.ToString(@"hh\:mm") ?? "";
                    worksheet.Cell(row, 11).Value = permiso.motivo;
                    worksheet.Cell(row, 12).Value = permiso.comentarios;
                    worksheet.Cell(row, 13).Value = string.IsNullOrEmpty(permiso.nombreArchivo) ? "No" : "Sí";

                    row++;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                // Preparar respuesta
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Permisos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el archivo Excel: {ex.Message}");
            }
        }

        public async Task<IActionResult> ExportarPermisosPDF(
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
                var query = _context.Permiso.AsNoTracking().AsQueryable();

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

                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (fechaTipo == "single" && !string.IsNullOrEmpty(fechaUnica))
                    {
                        if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDate))
                        {
                            query = query.Where(p =>
                                p.fechaCreacion.Date == fechaUnicaDate.Date ||
                                p.fechaCreacion.Date == fechaUnicaDate.Date);
                        }
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateTime.TryParse(fechaDesde, out DateTime desdeDate) &&
                            DateTime.TryParse(fechaHasta, out DateTime hastaDate))
                        {
                            query = query.Where(p =>
                                (p.fechaCreacion.Date >= desdeDate.Date && p.fechaCreacion.Date <= hastaDate.Date) ||
                                (p.fechaCreacion.Date >= desdeDate.Date && p.fechaCreacion.Date <= hastaDate.Date));
                        }
                    }
                }

                var permisos = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(p => new
                    {
                        p.idPermiso,
                        p.nombreEmpleado,
                        p.departamento,
                        p.tipo,
                        p.estado,
                        p.fechaCreacion,
                        p.fechaInicio,
                        p.fechaFinalizacion,
                        p.fechaRegresoLaboral,
                        p.horaCita,
                        p.motivo,
                        p.comentarios,
                        p.nombreArchivo
                    })
                    .ToListAsync();

                // Crear PDF
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Título - CORREGIDO
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var titulo = new Paragraph("REPORTE DE PERMISOS", tituloFont);
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
                AddTableCell(table, "Motivo", headerFont);
                AddTableCell(table, "Comentarios", headerFont);

                // Datos - CORREGIDO
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                foreach (var permiso in permisos)
                {
                    AddTableCell(table, permiso.idPermiso.ToString(), cellFontBold);
                    AddTableCell(table, permiso.nombreEmpleado ?? "", cellFont);
                    AddTableCell(table, permiso.departamento ?? "", cellFont);
                    AddTableCell(table, permiso.tipo ?? "", cellFont);

                    // Color según estado
                    var estadoFont = GetEstadoFont(permiso.estado);
                    AddTableCell(table, permiso.estado ?? "", estadoFont);

                    var horaCitaStr = permiso.horaCita?.ToString(@"hh\:mm") ?? "";

                    var fechas = $"Inicio: {permiso.fechaInicio:dd/MM/yyyy}\nFin: {permiso.fechaFinalizacion:dd/MM/yyyy}\nRegreso: {permiso.fechaRegresoLaboral:dd/MM/yyyy}\nHora: {horaCitaStr}";
                        AddTableCell(table, fechas, cellFont);

                    AddTableCell(table, permiso.motivo ?? "Sin motivo", cellFont);
                    AddTableCell(table, permiso.comentarios ?? "Sin comentarios", cellFont);
                }

                document.Add(table);

                // Pie de página - CORREGIDO
                var pieFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC);
                var pie = new Paragraph($"Total de registros: {permisos.Count}", pieFont);
                pie.Alignment = Element.ALIGN_RIGHT;
                pie.SpacingBefore = 10f;
                document.Add(pie);

                document.Close();

                // Devolver PDF
                var fileName = $"Permisos_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
                    var contadores = await _context.Permiso
                        .AsNoTracking()
                        .GroupBy(p => 1)
                        .Select(g => new
                        {
                            // Contadores por tipo
                            CitaMedica = g.Count(p => p.tipo == "Cita Médica"),
                            Vacaciones = g.Count(p => p.tipo == "Vacaciones"),
                            Incapacidad = g.Count(p => p.tipo == "Incapacidad"),
                            Teletrabajo = g.Count(p => p.tipo == "Teletrabajo"),
                            Especial = g.Count(p => p.tipo == "Especial"),

                            // Contadores por estado
                            Aprobado = g.Count(p => p.estado == "Creada" || p.estado == "Aprobada"),
                            Pendiente = g.Count(p => p.estado == "Pendiente"),
                            Rechazado = g.Count(p => p.estado == "Rechazada"),

                            // Contadores por departamento ← AGREGAR ESTOS
                            FinancieroContable = g.Count(p => p.departamento == "Financiero Contable"),
                            Gerencia = g.Count(p => p.departamento == "Gerencia"),
                            Ingenieria = g.Count(p => p.departamento == "Ingenieria"),
                            Jefatura = g.Count(p => p.departamento == "Jefatura"),
                            Legal = g.Count(p => p.departamento == "Legal"),
                            Operaciones = g.Count(p => p.departamento == "Operaciones"),
                            TecnicosNCR = g.Count(p => p.departamento == "Tecnicos NCR"),
                            TecnologiasInformacion = g.Count(p => p.departamento == "Tecnologias de Informacion"),
                            Ventas = g.Count(p => p.departamento == "Ventas")
                        })
                        .FirstOrDefaultAsync();

                    return Ok(contadores ?? new
                    {
                        CitaMedica = 0,
                        Vacaciones = 0,
                        Incapacidad = 0,
                        Teletrabajo = 0,
                        Especial = 0,
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
                        CitaMedica = 0,
                        Vacaciones = 0,
                        Incapacidad = 0,
                        Teletrabajo = 0,
                        Especial = 0,
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
        [Route("Permiso/Details/{id}")]
        public async Task<ActionResult> Details(long id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de permiso válido.";
                return RedirectToAction("VerPermisos", "Permiso");
            }

            var permiso = await _context.Permiso.FindAsync(id);

            if (permiso == null)
            {
                TempData["ErrorMessage"] = "El permiso que intentas editar no existe o fue eliminado.";
                return RedirectToAction("VerPermisos", "Permiso");
            }

            return View(permiso);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR ADJUNTO DE PERMISO
        [HttpGet]
        [Route("Permiso/descargar-adjunto/{id}")]
        public async Task<IActionResult> DescargarAdjunto(long id)
        {
            var permiso = await _context.Permiso
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.idPermiso == id);

            if (permiso?.datosAdjuntos == null || permiso.datosAdjuntos.Length == 0)
            {
                return NotFound("No se encontró el archivo adjunto");
            }

            // Retornar el archivo
            return File(
                permiso.datosAdjuntos,
                permiso.tipoMIME ?? "application/octet-stream",
                permiso.nombreArchivo ?? "archivo_adjunto"
            );
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // HACER REGISTRO DE UN NUEVO PERMISO
        [HttpPost]
        public async Task<IActionResult> Create(Permiso model)
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

                var nuevo = new Permiso
                {
                    fechaCreacion = ahoraCR,
                    nombreEmpleado = user.nombreCompleto,
                    departamento = user.departamento,
                    tipo = model.tipo,

                    fechaInicio = model.fechaInicio,
                    fechaFinalizacion = model.fechaFinalizacion,
                    fechaRegresoLaboral = model.fechaRegresoLaboral,
                    horaCita = model.horaCita,

                    motivo = model.motivo,
                    comentarios = model.comentarios,

                    foto = fotoService.ConvertFileToByteArrayAsync(model.fotoFile).Result,

                    datosAdjuntos = await adjuntoService.ProcesarArchivoAdjunto(model.adjuntoFile),
                    nombreArchivo = model.adjuntoFile?.FileName,
                    tipoMIME = model.adjuntoFile?.ContentType,
                    tamanoArchivo = model.adjuntoFile?.Length,

                    estado = "Creada"
                };

                _context.Permiso.Add(nuevo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Se creó el permiso correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error en la creación del permiso.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}