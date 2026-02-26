//using ClosedXML.Excel;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SistemasAnaliticos.Entidades;
//using SistemasAnaliticos.Helpers;
//using SistemasAnaliticos.Models;
//using SistemasAnaliticos.Services;
//using SistemasAnaliticos.ViewModels;
//using System.Runtime.InteropServices;

//namespace SistemasAnaliticos.Controllers
//{
//    public class GarantiaController : Controller
//    {
//        private readonly DBContext _context;
//        private readonly UserManager<Usuario> _userManager;
//        private readonly IPermisoAlcanceService _permisoAlcanceService;
//        private readonly IEmailService _emailService;

//        public GarantiaController(DBContext context, UserManager<Usuario> userManager, IPermisoAlcanceService permisoAlcanceService, IEmailService emailService)
//        {
//            _context = context;
//            _userManager = userManager;
//            _permisoAlcanceService = permisoAlcanceService;
//            _emailService = emailService;
//        }

//        // SOLO VISTA DE BOTONES
//        //[Authorize(Policy = "Garantia.Crear")]
//        public async Task<IActionResult> Index()
//        {
//            return View();
//        }
//        // --------------------------------------------------------------

//        // -------------------------------------------------------------------------------------------------------------------------------
//        // INDEX DONDE SE VEN LOS PERMISOS
//        [Authorize(Policy = "Garantia.Ver")]
//        public async Task<IActionResult> VerGarantias(int page = 1)
//        {
//            int pageSize = 3;

//            var query = _context.Garantia.AsNoTracking();
//            query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

//            var totalGarantias = await query.CountAsync();

//            var garantias = await query
//                .OrderByDescending(p => p.fechaCreacion)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .Select(p => new GarantiaViewModel
//                {
//                    idPermiso = p.idPermiso,
//                    fechaCreacion = p.fechaCreacion,
//                    nombreEmpleado = p.nombreEmpleado,
//                    departamento = p.departamento,
//                    tipo = p.tipo,
//                    fechaInicio = p.fechaInicio,
//                    fechaFinalizacion = p.fechaFinalizacion,
//                    fechaRegresoLaboral = p.fechaRegresoLaboral,
//                    horaCita = p.horaCita,
//                    motivo = p.motivo,
//                    comentarios = p.comentarios,
//                    foto = p.foto,
//                    nombreArchivo = p.nombreArchivo,
//                    tipoMIME = p.tipoMIME,
//                    tamanoArchivo = p.tamanoArchivo,
//                    estado = p.estado
//                })
//                .ToListAsync();

//            return View(new PaginacionGarantiasViewModel
//            {
//                Garantias = garantias,
//                PaginaActual = page,
//                TotalPaginas = (int)Math.Ceiling(totalGarantias / (double)pageSize)
//            });
//        }

//        // -------------------------------------------------------------------------------------------------------------------------------
//        // ENDPOINT PARA TENER TODOS LO PERMISOS (sin paginación)
//        public async Task<IActionResult> ObtenerTodosLosPermisos()
//        {
//            try
//            {
//                var query = _context.Permiso.AsNoTracking();
//                query = await _permisoAlcanceService.AplicarAlcancePermisoAsync(query, User);

//                var todosLosPermisos = await query
//                    .AsNoTracking()
//                    .OrderByDescending(x => x.fechaCreacion)
//                    .Select(p => new
//                    {
//                        p.idPermiso,
//                        p.nombreEmpleado,
//                        p.departamento,
//                        p.tipo,
//                        p.estado,
//                        p.fechaCreacion,
//                        p.fechaInicio,
//                        p.fechaFinalizacion,
//                        p.fechaRegresoLaboral,
//                        p.horaCita,
//                        p.motivo,
//                        p.comentarios,
//                        p.foto,
//                        p.nombreArchivo,
//                        p.tipoMIME,
//                        p.tamanoArchivo
//                    })
//                    .ToListAsync();

//                return Ok(todosLosPermisos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, "Error interno del servidor");
//            }
//        }

//        // -------------------------------------------------------------------------------------------------------------------------------
//        // METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
//        public async Task<IActionResult> ObtenerPermisosFiltradosCompletos(
//            [FromQuery] string[] tipos,
//            [FromQuery] string[] estados,
//            [FromQuery] string[] departamentos,
//            [FromQuery] string nombre = null,
//            [FromQuery] string fechaTipo = null,
//            [FromQuery] string fechaUnica = null,
//            [FromQuery] string fechaDesde = null,
//            [FromQuery] string fechaHasta = null)
//        {
//            try
//            {
//                var query = _context.Permiso.AsNoTracking();
//                query = await _permisoAlcanceService.AplicarAlcancePermisoAsync(query, User);

//                // Aplicar filtros de tipo
//                if (tipos != null && tipos.Length > 0)
//                {
//                    query = query.Where(p => tipos.Contains(p.tipo));
//                }

//                // Aplicar filtros de estado
//                if (estados != null && estados.Length > 0)
//                {
//                    var estadosMapeados = estados.Select(e =>
//                        e == "Aprobada" ? "Aprobada" :
//                        e == "Creada" ? "Creada" :
//                        e == "Pendiente" ? "Pendiente" :
//                        "Rechazada").ToArray();

//                    query = query.Where(p => estadosMapeados.Contains(p.estado));
//                }

//                // Aplicar filtros de departamento
//                if (departamentos != null && departamentos.Length > 0)
//                {
//                    query = query.Where(p => departamentos.Contains(p.departamento));
//                }

//                // Aplicar filtro por nombre
//                if (!string.IsNullOrWhiteSpace(nombre))
//                {
//                    query = query.Where(p => !string.IsNullOrEmpty(p.nombreEmpleado) && p.nombreEmpleado.Contains(nombre));
//                }

//                // Aplicar filtros de fecha
//                if (!string.IsNullOrEmpty(fechaTipo))
//                {
//                    if (fechaTipo == "single" && !string.IsNullOrEmpty(fechaUnica))
//                    {
//                        if (DateOnly.TryParse(fechaUnica, out DateOnly fechaUnicaDate))
//                        {
//                            query = query.Where(p => p.fechaCreacion == fechaUnicaDate);
//                        }
//                    }

//                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
//                    {
//                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
//                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
//                        {
//                            query = query.Where(p =>
//                                p.fechaCreacion >= desdeDate &&
//                                p.fechaCreacion <= hastaDate
//                            );
//                        }
//                    }
//                }

//                var permisos = await query
//                    .OrderByDescending(x => x.fechaCreacion)
//                    .Select(p => new
//                    {
//                        p.idPermiso,
//                        p.nombreEmpleado,
//                        p.departamento,
//                        p.tipo,
//                        p.estado,
//                        p.fechaCreacion,
//                        p.fechaInicio,
//                        p.fechaFinalizacion,
//                        p.fechaRegresoLaboral,
//                        p.horaCita,
//                        p.motivo,
//                        p.comentarios,
//                        p.foto,
//                        p.nombreArchivo,
//                        p.tipoMIME,
//                        p.tamanoArchivo
//                    })
//                    .ToListAsync();

//                return Ok(permisos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, "Error interno del servidor al aplicar filtros");
//            }
//        }

//        // -------------------------------------------------------------------------------------------------------------------------------
//        // DESCARGAR PERMISOS FILTRADOS EN EXCEL Y PDF
//        public async Task<IActionResult> ExportarPermisosExcel(
//        [FromQuery] string[] tipos,
//        [FromQuery] string[] estados,
//        [FromQuery] string[] departamentos,
//        [FromQuery] string nombre = null,
//        [FromQuery] string fechaTipo = null,
//        [FromQuery] string fechaUnica = null,
//        [FromQuery] string fechaDesde = null,
//        [FromQuery] string fechaHasta = null)
//        {
//            try
//            {
//                var query = _context.Permiso.AsNoTracking();
//                query = await _permisoAlcanceService.AplicarAlcancePermisoAsync(query, User);

//                // Aplicar filtros de tipo
//                if (tipos != null && tipos.Length > 0)
//                {
//                    query = query.Where(p => tipos.Contains(p.tipo));
//                }

//                // Aplicar filtros de estado - SIN mapeo, directo
//                if (estados != null && estados.Length > 0)
//                {
//                    query = query.Where(p => estados.Contains(p.estado));
//                }

//                // Aplicar filtros de departamento
//                if (departamentos != null && departamentos.Length > 0)
//                {
//                    query = query.Where(p => departamentos.Contains(p.departamento));
//                }

//                // Aplicar filtro por nombre
//                if (!string.IsNullOrWhiteSpace(nombre))
//                {
//                    query = query.Where(p => p.nombreEmpleado != null && p.nombreEmpleado.ToLower().Contains(nombre.ToLower()));
//                }

//                // Aplicar filtros de fecha
//                if (!string.IsNullOrEmpty(fechaTipo))
//                {
//                    if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDT))
//                    {
//                        DateOnly fechaUnicaDate = DateOnly.FromDateTime(fechaUnicaDT);
//                        query = query.Where(p => p.fechaCreacion == fechaUnicaDate);
//                    }
//                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
//                    {
//                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
//                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
//                        {
//                            query = query.Where(p =>
//                                p.fechaCreacion >= desdeDate &&
//                                p.fechaCreacion <= hastaDate
//                            );
//                        }
//                    }
//                }

//                var permisos = await query
//                    .OrderByDescending(x => x.fechaCreacion)
//                    .Select(p => new
//                    {
//                        p.idPermiso,
//                        p.nombreEmpleado,
//                        p.departamento,
//                        p.tipo,
//                        p.estado,
//                        p.fechaCreacion,
//                        p.fechaInicio,
//                        p.fechaFinalizacion,
//                        p.fechaRegresoLaboral,
//                        p.horaCita,
//                        p.motivo,
//                        p.comentarios,
//                        p.nombreArchivo
//                    })
//                    .ToListAsync();

//                // Crear el libro de Excel
//                using var workbook = new ClosedXML.Excel.XLWorkbook();
//                var worksheet = workbook.Worksheets.Add("Permisos");

//                // Estilos para el encabezado
//                var headerStyle = workbook.Style;
//                headerStyle.Font.Bold = true;
//                headerStyle.Fill.BackgroundColor = XLColor.LightGray;
//                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

//                // Encabezados
//                worksheet.Cell(1, 1).Value = "ID Permiso";
//                worksheet.Cell(1, 2).Value = "Nombre Empleado";
//                worksheet.Cell(1, 3).Value = "Departamento";
//                worksheet.Cell(1, 4).Value = "Tipo";
//                worksheet.Cell(1, 5).Value = "Estado";
//                worksheet.Cell(1, 6).Value = "Fecha Creación";
//                worksheet.Cell(1, 7).Value = "Fecha Inicio";
//                worksheet.Cell(1, 8).Value = "Fecha Finalización";
//                worksheet.Cell(1, 9).Value = "Fecha Regreso Laboral";
//                worksheet.Cell(1, 10).Value = "Hora Cita";
//                worksheet.Cell(1, 11).Value = "Motivo";
//                worksheet.Cell(1, 12).Value = "Comentarios";
//                worksheet.Cell(1, 13).Value = "Archivo Adjunto";

//                // Aplicar estilo al encabezado
//                worksheet.Range(1, 1, 1, 13).Style = headerStyle;

//                // Llenar datos
//                int row = 2;
//                foreach (var permiso in permisos)
//                {
//                    worksheet.Cell(row, 1).Value = permiso.idPermiso;
//                    worksheet.Cell(row, 2).Value = permiso.nombreEmpleado;
//                    worksheet.Cell(row, 3).Value = permiso.departamento;
//                    worksheet.Cell(row, 4).Value = permiso.tipo;
//                    worksheet.Cell(row, 5).Value = permiso.estado; // Estado directo, sin mapeo
//                    worksheet.Cell(row, 6).Value = permiso.fechaCreacion.ToString("yyyy-MM-dd");
//                    worksheet.Cell(row, 7).Value = permiso.fechaInicio?.ToString("yyyy-MM-dd");
//                    worksheet.Cell(row, 8).Value = permiso.fechaFinalizacion?.ToString("yyyy-MM-dd");
//                    worksheet.Cell(row, 9).Value = permiso.fechaRegresoLaboral?.ToString("yyyy-MM-dd");
//                    worksheet.Cell(row, 10).Value = permiso.horaCita?.ToString(@"hh\:mm") ?? "";
//                    worksheet.Cell(row, 11).Value = permiso.motivo;
//                    worksheet.Cell(row, 12).Value = permiso.comentarios;
//                    worksheet.Cell(row, 13).Value = string.IsNullOrEmpty(permiso.nombreArchivo) ? "No" : "Sí";

//                    row++;
//                }

//                // Ajustar ancho de columnas
//                worksheet.Columns().AdjustToContents();

//                // Preparar respuesta
//                var stream = new MemoryStream();
//                workbook.SaveAs(stream);
//                stream.Position = 0;

//                var fileName = $"Permisos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

//                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
//                    ? "Central America Standard Time"           // Windows
//                    : "America/Costa_Rica";                     // Linux/macOS

//                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
//                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
//                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
//                var usuario = await _userManager.GetUserAsync(User);

//                // Auditoría
//                var auditoria = new Auditoria
//                {
//                    Fecha = hoy,
//                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
//                    Usuario = usuario.nombreCompleto ?? "Desconocido",
//                    Tabla = "Permiso",
//                    Accion = "Exportación Excel"
//                };
//                _context.Auditoria.Add(auditoria);

//                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al generar el archivo Excel: {ex.Message}");
//            }
//        }

//        public async Task<IActionResult> ExportarPermisosPDF(
//        [FromQuery] string[] tipos,
//        [FromQuery] string[] estados,
//        [FromQuery] string[] departamentos,
//        [FromQuery] string nombre = null,
//        [FromQuery] string fechaTipo = null,
//        [FromQuery] string fechaUnica = null,
//        [FromQuery] string fechaDesde = null,
//        [FromQuery] string fechaHasta = null)
//        {
//            try
//            {
//                var query = _context.Permiso.AsNoTracking();
//                query = await _permisoAlcanceService.AplicarAlcancePermisoAsync(query, User);

//                if (tipos != null && tipos.Length > 0)
//                {
//                    query = query.Where(p => tipos.Contains(p.tipo));
//                }

//                if (estados != null && estados.Length > 0)
//                {
//                    query = query.Where(p => estados.Contains(p.estado));
//                }

//                if (departamentos != null && departamentos.Length > 0)
//                {
//                    query = query.Where(p => departamentos.Contains(p.departamento));
//                }

//                if (!string.IsNullOrWhiteSpace(nombre))
//                {
//                    query = query.Where(p => p.nombreEmpleado != null && p.nombreEmpleado.ToLower().Contains(nombre.ToLower()));
//                }

//                if (!string.IsNullOrEmpty(fechaTipo))
//                {
//                    if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDT))
//                    {
//                        DateOnly fechaUnicaDate = DateOnly.FromDateTime(fechaUnicaDT);
//                        query = query.Where(p => p.fechaCreacion == fechaUnicaDate);
//                    }
//                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
//                    {
//                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
//                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
//                        {
//                            query = query.Where(p =>
//                                p.fechaCreacion >= desdeDate &&
//                                p.fechaCreacion <= hastaDate
//                            );
//                        }
//                    }
//                }

//                var permisos = await query
//                    .OrderByDescending(x => x.fechaCreacion)
//                    .Select(p => new
//                    {
//                        p.idPermiso,
//                        p.nombreEmpleado,
//                        p.departamento,
//                        p.tipo,
//                        p.estado,
//                        p.fechaCreacion,
//                        p.fechaInicio,
//                        p.fechaFinalizacion,
//                        p.fechaRegresoLaboral,
//                        p.horaCita,
//                        p.motivo,
//                        p.comentarios,
//                        p.nombreArchivo
//                    })
//                    .ToListAsync();

//                // Crear PDF
//                using var memoryStream = new MemoryStream();
//                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
//                var writer = PdfWriter.GetInstance(document, memoryStream);

//                document.Open();

//                // Título
//                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
//                var titulo = new Paragraph("REPORTE DE PERMISOS", tituloFont);
//                titulo.Alignment = Element.ALIGN_CENTER;
//                titulo.SpacingAfter = 20f;
//                document.Add(titulo);

//                // Fecha de generación
//                var fechaFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC);
//                var fechaGeneracion = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fechaFont);
//                fechaGeneracion.Alignment = Element.ALIGN_RIGHT;
//                fechaGeneracion.SpacingAfter = 10f;
//                document.Add(fechaGeneracion);

//                // Crear tabla
//                var table = new PdfPTable(8);
//                table.WidthPercentage = 100;
//                table.SetWidths(new float[] { 0.8f, 1.5f, 1.5f, 1.2f, 1.2f, 1.5f, 2f, 2f });

//                // Encabezados de tabla
//                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
//                AddTableCell(table, "ID", headerFont);
//                AddTableCell(table, "Empleado", headerFont);
//                AddTableCell(table, "Departamento", headerFont);
//                AddTableCell(table, "Tipo", headerFont);
//                AddTableCell(table, "Estado", headerFont);
//                AddTableCell(table, "Fechas", headerFont);
//                AddTableCell(table, "Motivo", headerFont);
//                AddTableCell(table, "Comentarios", headerFont);

//                // Datos
//                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
//                var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

//                foreach (var permiso in permisos)
//                {
//                    AddTableCell(table, permiso.idPermiso.ToString(), cellFontBold);
//                    AddTableCell(table, permiso.nombreEmpleado ?? "", cellFont);
//                    AddTableCell(table, permiso.departamento ?? "", cellFont);
//                    AddTableCell(table, permiso.tipo ?? "", cellFont);

//                    // Color según estado
//                    var estadoFont = GetEstadoFont(permiso.estado);
//                    AddTableCell(table, permiso.estado ?? "", estadoFont);

//                    var horaCitaStr = permiso.horaCita?.ToString(@"hh\:mm") ?? "";

//                    var fechas = $"Creación: {permiso.fechaCreacion:dd/MM/yyyy}\nInicio: {permiso.fechaInicio:dd/MM/yyyy}\nFin: {permiso.fechaFinalizacion:dd/MM/yyyy}\nRegreso: {permiso.fechaRegresoLaboral:dd/MM/yyyy}\nHora: {horaCitaStr}";
//                    AddTableCell(table, fechas, cellFont);

//                    AddTableCell(table, permiso.motivo ?? "Sin motivo", cellFont);
//                    AddTableCell(table, permiso.comentarios ?? "Sin comentarios", cellFont);
//                }

//                document.Add(table);

//                // Pie de página
//                var pieFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC);
//                var pie = new Paragraph($"Total de registros: {permisos.Count}", pieFont);
//                pie.Alignment = Element.ALIGN_RIGHT;
//                pie.SpacingBefore = 10f;
//                document.Add(pie);

//                document.Close();

//                // Devolver PDF
//                var fileName = $"Permisos_{DateTime.Now:yyyyMMddHHmmss}.pdf";

//                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
//                    ? "Central America Standard Time"           // Windows
//                    : "America/Costa_Rica";                     // Linux/macOS

//                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
//                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
//                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
//                var usuario = await _userManager.GetUserAsync(User);

//                // Auditoría
//                var auditoria = new Auditoria
//                {
//                    Fecha = hoy,
//                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
//                    Usuario = usuario.nombreCompleto ?? "Desconocido",
//                    Tabla = "Permiso",
//                    Accion = "Exportación PDF"
//                };
//                _context.Auditoria.Add(auditoria);

//                return File(memoryStream.ToArray(), "application/pdf", fileName);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
//            }
//        }

//        // Métodos auxiliares - CORREGIDOS
//        private void AddTableCell(PdfPTable table, string text, Font font)
//        {
//            var cell = new PdfPCell(new Phrase(text, font));
//            cell.Padding = 5;
//            cell.BorderWidth = 0.5f;
//            table.AddCell(cell);
//        }

//        private Font GetEstadoFont(string estado)
//        {
//            var baseFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

//            if (string.IsNullOrEmpty(estado)) return baseFont;

//            return estado.ToUpper() switch
//            {
//                "APROBADO" or "CREADA" or "APROBADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Green),
//                "PENDIENTE" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Orange),
//                "RECHAZADO" or "RECHAZADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Red),
//                _ => baseFont
//            };
//        }

//        // -------------------------------------------------------------------------------------------------------------------------------
//        // ENDPOINT PARA OBTENER CONTADORES DE FILTROS
//        public async Task<IActionResult> ObtenerContadoresFiltros()
//        {
//            try
//            {
//                var query = _context.Permiso.AsNoTracking();
//                query = await _permisoAlcanceService.AplicarAlcancePermisoAsync(query, User);

//                var contadores = await query
//                    .AsNoTracking()
//                    .GroupBy(p => 1)
//                    .Select(g => new
//                    {
//                        // Contadores por tipo
//                        CitaMedica = g.Count(p => p.tipo == "Cita Médica"),
//                        Vacaciones = g.Count(p => p.tipo == "Vacaciones"),
//                        Incapacidad = g.Count(p => p.tipo == "Incapacidad"),
//                        Teletrabajo = g.Count(p => p.tipo == "Teletrabajo"),
//                        Especial = g.Count(p => p.tipo == "Especial"),

//                        // Contadores por estado
//                        Creada = g.Count(p => p.estado == "Creada"),
//                        Aprobada = g.Count(p => p.estado == "Aprobada"),
//                        Pendiente = g.Count(p => p.estado == "Pendiente"),
//                        Rechazada = g.Count(p => p.estado == "Rechazada"),

//                        // Contadores por departamento ← AGREGAR ESTOS
//                        Bodega = g.Count(p => p.departamento == "Bodega"),
//                        FinancieroContable = g.Count(p => p.departamento == "Financiero Contable"),
//                        Gerencia = g.Count(p => p.departamento == "Gerencia"),
//                        Ingenieria = g.Count(p => p.departamento == "Ingeniería"),
//                        Jefatura = g.Count(p => p.departamento == "Jefatura"),
//                        Legal = g.Count(p => p.departamento == "Legal"),
//                        Operaciones = g.Count(p => p.departamento == "Operaciones"),
//                        RecursosHumanos = g.Count(p => p.departamento == "Recursos Humanos"),
//                        ServiciosGenerales = g.Count(b => b.departamento == "Servicios Generales"),
//                        TecnicosNCR = g.Count(p => p.departamento == "Técnicos NCR"),
//                        TecnologiasInformacion = g.Count(p => p.departamento == "Tecnologías de Información"),
//                        Ventas = g.Count(p => p.departamento == "Ventas")
//                    })
//                    .FirstOrDefaultAsync();

//                return Ok(contadores ?? new
//                {
//                    CitaMedica = 0,
//                    Vacaciones = 0,
//                    Incapacidad = 0,
//                    Teletrabajo = 0,
//                    Especial = 0,
//                    Creada = 0,
//                    Aprobada = 0,
//                    Pendiente = 0,
//                    Rechazada = 0,
//                    Bodega = 0,
//                    FinancieroContable = 0,
//                    Gerencia = 0,
//                    Ingenieria = 0,
//                    Jefatura = 0,
//                    Legal = 0,
//                    Operaciones = 0,
//                    RecursosHumanos = 0,
//                    ServiciosGenerales = 0,
//                    TecnicosNCR = 0,
//                    TecnologiasInformacion = 0,
//                    Ventas = 0
//                });
//            }
//            catch (Exception ex)
//            {
//                return Ok(new
//                {
//                    CitaMedica = 0,
//                    Vacaciones = 0,
//                    Incapacidad = 0,
//                    Teletrabajo = 0,
//                    Especial = 0,
//                    Creada = 0,
//                    Aprobada = 0,
//                    Pendiente = 0,
//                    Rechazada = 0,
//                    Bodega = 0,
//                    FinancieroContable = 0,
//                    Gerencia = 0,
//                    Ingenieria = 0,
//                    Jefatura = 0,
//                    Legal = 0,
//                    Operaciones = 0,
//                    RecursosHumanos = 0,
//                    ServiciosGenerales = 0,
//                    TecnicosNCR = 0,
//                    TecnologiasInformacion = 0,
//                    Ventas = 0
//                });
//            }
//        }




//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        //[Authorize(Policy = "Garantia.Crear")]
//        public async Task<IActionResult> Create(Garantia model)
//        {
//            var usuario = await _userManager.GetUserAsync(User);

//            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
//                ? "Central America Standard Time"           // Windows
//                : "America/Costa_Rica";                     // Linux/macOS

//            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
//            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);

//            try
//            {
//                // Crear nueva garantía con los datos del formulario
//                var nuevaGarantia = new Garantia
//                {
//                    UsuarioId = usuario.Id,
//                    fechaCreacion = ahoraCR,
//                    nombreEmpleado = usuario.nombreCompleto,
//                    departamento = usuario.departamento,

//                    // Datos del paso 1
//                    moneda = model.moneda,
//                    monto = model.monto,
//                    aFavorDe = model.aFavorDe,
//                    nombreLicitacion = model.nombreLicitacion,
//                    prorroga = model.prorroga,
//                    numeroGarantia = model.numeroGarantia,
//                    numeroLicitacion = model.numeroLicitacion,

//                    // Datos del paso 2
//                    tipoLicitacion = model.tipoLicitacion,
//                    fechaInicio = model.fechaInicio,
//                    fechaFinalizacion = model.fechaFinalizacion,
//                    plazo = model.plazo,
//                    observacion = model.observacion,

//                    // Estado inicial
//                    estado = "Creada"
//                };

//                // Procesar adjuntos (hasta 5 archivos)
//                var adjuntos = new[]
//                {
//                    model.adjuntoFile1,
//                    model.adjuntoFile2,
//                    model.adjuntoFile3,
//                    model.adjuntoFile4,
//                    model.adjuntoFile5
//                };

//                for (int i = 0; i < adjuntos.Length; i++)
//                {
//                    var archivo = adjuntos[i];
//                    if (archivo != null && archivo.Length > 0)
//                    {
//                        // Validar tamaño (máximo 10MB) - ya validado por el atributo MaxFileSize
//                        // Validar extensión
//                        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
//                        var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

//                        if (!extensionesPermitidas.Contains(extension))
//                        {
//                            TempData["WarningMessage"] = $"El archivo {archivo.FileName} tiene formato no permitido y no fue adjuntado.";
//                            continue;
//                        }

//                        // Convertir archivo a byte array
//                        using (var memoryStream = new MemoryStream())
//                        {
//                            await archivo.CopyToAsync(memoryStream);
//                            var archivoBytes = memoryStream.ToArray();

//                            // Asignar según el índice del adjunto
//                            switch (i)
//                            {
//                                case 0:
//                                    nuevaGarantia.datosAdjuntos1 = archivoBytes;
//                                    nuevaGarantia.nombreArchivo1 = archivo.FileName;
//                                    nuevaGarantia.tipoMIME1 = archivo.ContentType;
//                                    nuevaGarantia.tamanoArchivo1 = archivo.Length;
//                                    break;
//                                case 1:
//                                    nuevaGarantia.datosAdjuntos2 = archivoBytes;
//                                    nuevaGarantia.nombreArchivo2 = archivo.FileName;
//                                    nuevaGarantia.tipoMIME2 = archivo.ContentType;
//                                    nuevaGarantia.tamanoArchivo2 = archivo.Length;
//                                    break;
//                                case 2:
//                                    nuevaGarantia.datosAdjuntos3 = archivoBytes;
//                                    nuevaGarantia.nombreArchivo3 = archivo.FileName;
//                                    nuevaGarantia.tipoMIME3 = archivo.ContentType;
//                                    nuevaGarantia.tamanoArchivo3 = archivo.Length;
//                                    break;
//                                case 3:
//                                    nuevaGarantia.datosAdjuntos4 = archivoBytes;
//                                    nuevaGarantia.nombreArchivo4 = archivo.FileName;
//                                    nuevaGarantia.tipoMIME4 = archivo.ContentType;
//                                    nuevaGarantia.tamanoArchivo4 = archivo.Length;
//                                    break;
//                                case 4:
//                                    nuevaGarantia.datosAdjuntos5 = archivoBytes;
//                                    nuevaGarantia.nombreArchivo5 = archivo.FileName;
//                                    nuevaGarantia.tipoMIME5 = archivo.ContentType;
//                                    nuevaGarantia.tamanoArchivo5 = archivo.Length;
//                                    break;
//                            }
//                        }
//                    }
//                }

//                _context.Garantia.Add(nuevaGarantia);
//                await _context.SaveChangesAsync();

//                try
//                {
//                    // 1. Correo al empleado
//                    var htmlEmpleado = PlantillasEmail.ConfirmacionEmpleadoGarantia(
//                        nombreEmpleado: usuario.nombreCompleto,
//                        nombreLicitacion: model.nombreLicitacion
//                    );

//                    //await _emailService.SendEmailAsync(
//                    //    toEmail: usuario.Email,
//                    //    toName: usuario.nombreCompleto,
//                    //    subject: $"Confirmación de Garantía - {usuario.nombreCompleto}",
//                    //    htmlBody: htmlEmpleado
//                    //);

//                    // 2. Correo de notificación a la persona de revisión (Ana Novo y Hillary Hernández)
//                    if (!string.IsNullOrEmpty(usuario.jefeId))
//                    {
//                        var htmlRevision1 = PlantillasEmail.NotificacionRevisionGarantia(
//                            nombreEmpleado: usuario.nombreCompleto,
//                            nombreLicitacion: model.nombreLicitacion
//                        );
//                        //await _emailService.SendEmailAsync(
//                        //    toEmail: "ana.novo@sistemasanaliticos.cr",
//                        //    toName: "Ana Novo",
//                        //    subject: $"Solicitud de Garantía Revisión - {usuario.nombreCompleto}",
//                        //    htmlBody: htmlRevision1
//                        //);

//                        var htmlRevision2 = PlantillasEmail.NotificacionRevisionGarantia(
//                            nombreEmpleado: usuario.nombreCompleto,
//                            nombreLicitacion: model.nombreLicitacion
//                        );
//                        //await _emailService.SendEmailAsync(
//                        //    toEmail: "hillary.hernandez@sistemasanaliticos.cr",
//                        //    toName: "Hillary Hernández",
//                        //    subject: $"Solicitud de Garantía Revisión - {usuario.nombreCompleto}",
//                        //    htmlBody: htmlRevision2
//                        //);
//                    }
//                }
//                catch (Exception exEmail)
//                {
//                    // Solo log si hay error en correo, no interrumpir
//                    Console.WriteLine($"Error enviando correo: {exEmail.Message}");
//                }

//                // Auditoría
//                var auditoria = new Auditoria
//                {
//                    Fecha = DateOnly.FromDateTime(ahoraCR),
//                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
//                    Usuario = usuario.nombreCompleto ?? "Desconocido",
//                    Tabla = "Garantia",
//                    Accion = "Nuevo Registro"
//                };
//                _context.Auditoria.Add(auditoria);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Se creó la garantía correctamente.";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                // Log del error
//                Console.WriteLine($"Error en creación de garantía: {ex.Message}");
//                TempData["ErrorMessage"] = "Error en la creación de la garantía.";
//                return RedirectToAction("Index");
//            }
//        }
//    }
//}
