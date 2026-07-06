using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Helpers;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.ViewModels;
using System.ClientModel.Primitives;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class GarantiaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IPermisoAlcanceService _permisoAlcanceService;
        private readonly IEmailService _emailService;

        public GarantiaController(DBContext context, UserManager<Usuario> userManager, IPermisoAlcanceService permisoAlcanceService, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _permisoAlcanceService = permisoAlcanceService;
            _emailService = emailService;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX DONDE SE VEN LOS PERMISOS
        [Authorize(Policy = "Garantia.Ver")]
        public async Task<IActionResult> VerGarantias(int page = 1)
        {
            int pageSize = 3;

            var query = _context.Garantia.AsNoTracking();
            query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

            var totalGarantias = await query.CountAsync();

            var garantias = await query
                .OrderByDescending(p => p.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GarantiaViewModel
                {
                    idGarantia = g.idGarantia,
                    fechaCreacion = g.fechaCreacion,
                    nombreEmpleado = g.nombreEmpleado,
                    departamento = g.departamento,
                    Moneda = g.moneda,
                    Monto = g.monto,
                    NombreLicitacion = g.nombreLicitacion,
                    TipoLicitacion = g.tipoLicitacion,
                    FechaInicio = g.fechaInicio,
                    FechaFinalizacion = g.fechaFinalizacion,
                    estado = g.estado
                })
                .ToListAsync();

            return View(new PaginacionGarantiasViewModel
            {
                Garantias = garantias,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalGarantias / (double)pageSize)
            });
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA TENER TODOS LO PERMISOS (sin paginación)
        public async Task<IActionResult> ObtenerTodasLasGarantias()
        {
            try
            {
                var query = _context.Garantia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

                var todasLasGarantias = await query
                    .AsNoTracking()
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(g => new
                    {
                        g.idGarantia,
                        g.fechaCreacion,
                        g.nombreEmpleado,
                        g.departamento,
                        g.moneda,
                        g.monto,
                        g.nombreLicitacion,
                        g.tipoLicitacion,
                        g.fechaInicio,
                        g.fechaFinalizacion,
                        g.estado
                    })
                    .ToListAsync();

                return Ok(todasLasGarantias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerGarantiasFiltradasCompletas(
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
                var query = _context.Garantia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipoLicitacion));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobada" ? "Aprobada" :
                        e == "Creada" ? "Creada" :
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
                        if (DateTime.TryParse(fechaUnica, out DateTime fechaUnicaDate))
                        {
                            query = query.Where(p => p.fechaCreacion.Date == fechaUnicaDate);
                        }
                    }

                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateTime.TryParse(fechaDesde, out DateTime desdeDate) &&
                            DateTime.TryParse(fechaHasta, out DateTime hastaDate))
                        {
                            query = query.Where(p =>
                                p.fechaCreacion >= desdeDate &&
                                p.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var garantias = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(g => new
                    {
                        g.idGarantia,
                        g.fechaCreacion,
                        g.nombreEmpleado,
                        g.departamento,
                        g.moneda,
                        g.monto,
                        g.nombreLicitacion,
                        g.tipoLicitacion,
                        g.fechaInicio,
                        g.fechaFinalizacion,
                        g.estado
                    })
                    .ToListAsync();

                return Ok(garantias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor al aplicar filtros");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR PERMISOS FILTRADOS EN EXCEL Y PDF
        public async Task<IActionResult> ExportarGarantiasExcel(
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
                var query = _context.Garantia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipoLicitacion));
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
                    if (DateOnly.TryParse(fechaUnica, out DateOnly fechaUnicaDate))
                    {
                        query = query.Where(p => DateOnly.FromDateTime(p.fechaCreacion) == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(p =>
                                DateOnly.FromDateTime(p.fechaCreacion) >= desdeDate &&
                                DateOnly.FromDateTime(p.fechaCreacion) <= hastaDate
                            );
                        }
                    }
                }

                var garantias = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(g => new
                    {
                        g.idGarantia,
                        g.fechaCreacion,
                        g.nombreEmpleado,
                        g.departamento,
                        g.moneda,
                        g.monto,
                        g.nombreLicitacion,
                        g.tipoLicitacion,
                        g.fechaInicio,
                        g.fechaFinalizacion,
                        g.observacion,
                        g.estado,

                        g.nombreArchivo1,
                        g.nombreArchivo2,
                        g.nombreArchivo3,
                        g.nombreArchivo4,
                        g.nombreArchivo5
                    })
                    .ToListAsync();

                // Crear el libro de Excel
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Garantías");

                // Estilos para el encabezado
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Encabezados
                worksheet.Cell(1, 1).Value = "ID Garantía";
                worksheet.Cell(1, 2).Value = "Nombre Empleado";
                worksheet.Cell(1, 3).Value = "Departamento";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Nombre";
                worksheet.Cell(1, 6).Value = "Moneda";
                worksheet.Cell(1, 7).Value = "Monto";
                worksheet.Cell(1, 8).Value = "Estado";
                worksheet.Cell(1, 9).Value = "Fecha Creación";
                worksheet.Cell(1, 10).Value = "Fecha Inicio";
                worksheet.Cell(1, 11).Value = "Fecha Finalización";
                worksheet.Cell(1, 12).Value = "Observación";
                worksheet.Cell(1, 13).Value = "Archivo Adjunto1";
                worksheet.Cell(1, 14).Value = "Archivo Adjunto2";
                worksheet.Cell(1, 15).Value = "Archivo Adjunto3";
                worksheet.Cell(1, 16).Value = "Archivo Adjunto4";
                worksheet.Cell(1, 17).Value = "Archivo Adjunto5";

                // Aplicar estilo al encabezado
                worksheet.Range(1, 1, 1, 13).Style = headerStyle;

                // Llenar datos
                int row = 2;
                foreach (var garantia in garantias)
                {
                    worksheet.Cell(row, 1).Value = garantia.idGarantia;
                    worksheet.Cell(row, 2).Value = garantia.nombreEmpleado;
                    worksheet.Cell(row, 3).Value = garantia.departamento;
                    worksheet.Cell(row, 4).Value = garantia.tipoLicitacion;
                    worksheet.Cell(row, 5).Value = garantia.nombreLicitacion;
                    worksheet.Cell(row, 6).Value = garantia.moneda;
                    worksheet.Cell(row, 7).Value = garantia.monto;
                    worksheet.Cell(row, 8).Value = garantia.estado; // Estado directo, sin mapeo
                    worksheet.Cell(row, 9).Value = garantia.fechaCreacion.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 10).Value = garantia.fechaInicio?.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 11).Value = garantia.fechaFinalizacion?.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 12).Value = garantia.observacion;
                    worksheet.Cell(row, 13).Value = string.IsNullOrEmpty(garantia.nombreArchivo1) ? "No" : "Sí";
                    worksheet.Cell(row, 14).Value = string.IsNullOrEmpty(garantia.nombreArchivo2) ? "No" : "Sí";
                    worksheet.Cell(row, 15).Value = string.IsNullOrEmpty(garantia.nombreArchivo3) ? "No" : "Sí";
                    worksheet.Cell(row, 16).Value = string.IsNullOrEmpty(garantia.nombreArchivo4) ? "No" : "Sí";
                    worksheet.Cell(row, 17).Value = string.IsNullOrEmpty(garantia.nombreArchivo5) ? "No" : "Sí";
                    row++;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                // Preparar respuesta
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Garantías_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

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
                    Tabla = "Garantía",
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

        public async Task<IActionResult> ExportarGarantiasPDF(
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
                var query = _context.Garantia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

                // Aplicar filtros de tipo
                if (tipos != null && tipos.Length > 0)
                {
                    query = query.Where(p => tipos.Contains(p.tipoLicitacion));
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
                    if (DateOnly.TryParse(fechaUnica, out DateOnly fechaUnicaDate))
                    {
                        query = query.Where(p => DateOnly.FromDateTime(p.fechaCreacion) == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(p =>
                                DateOnly.FromDateTime(p.fechaCreacion) >= desdeDate &&
                                DateOnly.FromDateTime(p.fechaCreacion) <= hastaDate
                            );
                        }
                    }
                }

                var garantias = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(g => new
                    {
                        g.idGarantia,
                        g.fechaCreacion,
                        g.nombreEmpleado,
                        g.departamento,
                        g.moneda,
                        g.monto,
                        g.nombreLicitacion,
                        g.tipoLicitacion,
                        g.fechaInicio,
                        g.fechaFinalizacion,
                        g.observacion,
                        g.estado,

                        g.nombreArchivo1,
                        g.nombreArchivo2,
                        g.nombreArchivo3,
                        g.nombreArchivo4,
                        g.nombreArchivo5
                    })
                    .ToListAsync();

                // Crear PDF
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Título
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var titulo = new Paragraph("REPORTE DE GARANTÍAS", tituloFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 20f;
                document.Add(titulo);

                // Fecha de generación
                var fechaFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC);
                var fechaGeneracion = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fechaFont);
                fechaGeneracion.Alignment = Element.ALIGN_RIGHT;
                fechaGeneracion.SpacingAfter = 10f;
                document.Add(fechaGeneracion);

                // Crear tabla
                var table = new PdfPTable(10);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.8f, 1.5f, 1.5f, 1.2f, 2f, 1f, 1.5f, 1.2f, 2f, 2f });

                // Encabezados de tabla
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                AddTableCell(table, "ID Garantía", headerFont);
                AddTableCell(table, "Empleado", headerFont);
                AddTableCell(table, "Departamento", headerFont);
                AddTableCell(table, "Tipo", headerFont);
                AddTableCell(table, "Nombre", headerFont);
                AddTableCell(table, "Moneda", headerFont);
                AddTableCell(table, "Monto", headerFont);
                AddTableCell(table, "Estado", headerFont);
                AddTableCell(table, "Fechas", headerFont);
                AddTableCell(table, "Observación", headerFont);

                // Datos
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                foreach (var garantia in garantias)
                {
                    AddTableCell(table, garantia.idGarantia.ToString(), cellFontBold);
                    AddTableCell(table, garantia.nombreEmpleado ?? "", cellFont);
                    AddTableCell(table, garantia.departamento ?? "", cellFont);
                    AddTableCell(table, garantia.tipoLicitacion ?? "", cellFont);
                    AddTableCell(table, garantia.nombreLicitacion ?? "", cellFont);
                    AddTableCell(table, garantia.moneda ?? "", cellFont);
                    AddTableCell(table, "₡ " + (garantia.monto).ToString("#,##0.00", new CultureInfo("es-CR")), cellFont);

                    // Color según estado
                    var estadoFont = GetEstadoFont(garantia.estado);
                    AddTableCell(table, garantia.estado ?? "", estadoFont);

                    var fechas = $"Creación: {garantia.fechaCreacion:dd/MM/yyyy HH:mm}\n" +
                                 $"Inicio: {(garantia.fechaInicio.HasValue ? garantia.fechaInicio.Value.ToString("dd/MM/yyyy") : "-")}\n" +
                                 $"Fin: {(garantia.fechaFinalizacion.HasValue ? garantia.fechaFinalizacion.Value.ToString("dd/MM/yyyy") : "-")}"; AddTableCell(table, fechas, cellFont);

                    AddTableCell(table, garantia.observacion ?? "Sin Observación", cellFont);
                }

                document.Add(table);

                // Pie de página
                var pieFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC);
                var pie = new Paragraph($"Total de registros: {garantias.Count}", pieFont);
                pie.Alignment = Element.ALIGN_RIGHT;
                pie.SpacingBefore = 10f;
                document.Add(pie);

                document.Close();

                // Devolver PDF
                var fileName = $"Garantías_{DateTime.Now:yyyyMMddHHmmss}.pdf";

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
                    Tabla = "Garantía",
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
                "CREADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Blue),
                "REVISADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Green),
                _ => baseFont
            };
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA OBTENER CONTADORES DE FILTROS
        public async Task<IActionResult> ObtenerContadoresFiltros()
        {
            try
            {
                var query = _context.Garantia.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceGarantiaAsync(query, User);

                var contadores = await query
                    .AsNoTracking()
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        // Contadores por tipo
                        LY = g.Count(p => p.tipoLicitacion == "LY"),
                        LE = g.Count(p => p.tipoLicitacion == "LE"),
                        LD = g.Count(p => p.tipoLicitacion == "LD"),
                        PX = g.Count(p => p.tipoLicitacion == "PX"),

                        // Contadores por estado
                        Creada = g.Count(p => p.estado == "Creada"),
                        Aprobada = g.Count(p => p.estado == "Aprobada"),
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
                        ServiciosGenerales = g.Count(b => b.departamento == "Servicios Generales"),
                        TecnicosNCR = g.Count(p => p.departamento == "Técnicos NCR"),
                        TecnologiasInformacion = g.Count(p => p.departamento == "Tecnologías de Información"),
                        Ventas = g.Count(p => p.departamento == "Ventas")
                    })
                    .FirstOrDefaultAsync();

                return Ok(contadores ?? new
                {
                    LY = 0,
                    LE = 0,
                    LD = 0,
                    PX = 0,
                    Creada = 0,
                    Aprobada = 0,
                    Rechazada = 0,
                    Bodega = 0,
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    RecursosHumanos = 0,
                    ServiciosGenerales = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    LY = 0,
                    LE = 0,
                    LD = 0,
                    PX = 0,
                    Creada = 0,
                    Aprobada = 0,
                    Rechazada = 0,
                    Bodega = 0,
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    RecursosHumanos = 0,
                    ServiciosGenerales = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Garantia.Crear")]
        public async Task<IActionResult> Create(Garantia model)
        {
            var usuario = await _userManager.GetUserAsync(User);

            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);

            try
            {
                // Crear nueva garantía con los datos del formulario
                var nuevaGarantia = new Garantia
                {
                    UsuarioId = usuario.Id,
                    fechaCreacion = ahoraCR,
                    nombreEmpleado = usuario.nombreCompleto,
                    departamento = usuario.departamento,

                    // Datos del paso 1
                    moneda = model.moneda,
                    monto = model.monto,
                    aFavorDe = model.aFavorDe,
                    nombreLicitacion = model.nombreLicitacion,
                    prorroga = model.prorroga,
                    numeroGarantia = model.numeroGarantia,
                    numeroLicitacion = model.numeroLicitacion,

                    // Datos del paso 2
                    tipoLicitacion = model.tipoLicitacion,
                    fechaInicio = model.fechaInicio,
                    fechaFinalizacion = model.fechaFinalizacion,
                    plazo = model.plazo,
                    observacion = model.observacion,

                    // Estado inicial
                    estado = "Creada"
                };

                // Procesar adjuntos (hasta 5 archivos)
                var adjuntos = new[]
                {
                    model.adjuntoFile1,
                    model.adjuntoFile2,
                    model.adjuntoFile3,
                    model.adjuntoFile4,
                    model.adjuntoFile5
                };

                for (int i = 0; i < adjuntos.Length; i++)
                {
                    var archivo = adjuntos[i];
                    if (archivo != null && archivo.Length > 0)
                    {
                        // Validar tamaño (máximo 10MB) - ya validado por el atributo MaxFileSize
                        // Validar extensión
                        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                        var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

                        if (!extensionesPermitidas.Contains(extension))
                        {
                            TempData["WarningMessage"] = $"El archivo {archivo.FileName} tiene formato no permitido y no fue adjuntado.";
                            continue;
                        }

                        // Convertir archivo a byte array
                        using (var memoryStream = new MemoryStream())
                        {
                            await archivo.CopyToAsync(memoryStream);
                            var archivoBytes = memoryStream.ToArray();

                            // Asignar según el índice del adjunto
                            switch (i)
                            {
                                case 0:
                                    nuevaGarantia.datosAdjuntos1 = archivoBytes;
                                    nuevaGarantia.nombreArchivo1 = archivo.FileName;
                                    nuevaGarantia.tipoMIME1 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo1 = archivo.Length;
                                    break;
                                case 1:
                                    nuevaGarantia.datosAdjuntos2 = archivoBytes;
                                    nuevaGarantia.nombreArchivo2 = archivo.FileName;
                                    nuevaGarantia.tipoMIME2 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo2 = archivo.Length;
                                    break;
                                case 2:
                                    nuevaGarantia.datosAdjuntos3 = archivoBytes;
                                    nuevaGarantia.nombreArchivo3 = archivo.FileName;
                                    nuevaGarantia.tipoMIME3 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo3 = archivo.Length;
                                    break;
                                case 3:
                                    nuevaGarantia.datosAdjuntos4 = archivoBytes;
                                    nuevaGarantia.nombreArchivo4 = archivo.FileName;
                                    nuevaGarantia.tipoMIME4 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo4 = archivo.Length;
                                    break;
                                case 4:
                                    nuevaGarantia.datosAdjuntos5 = archivoBytes;
                                    nuevaGarantia.nombreArchivo5 = archivo.FileName;
                                    nuevaGarantia.tipoMIME5 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo5 = archivo.Length;
                                    break;
                            }
                        }
                    }
                }

                _context.Garantia.Add(nuevaGarantia);
                await _context.SaveChangesAsync();

                try
                {
                    Console.WriteLine($"Envio de Correo");
                    //// 1. Correo al empleado
                    //var htmlEmpleado = PlantillasEmail.ConfirmacionCreacionGarantia(
                    //    nombreEmpleado: usuario.nombreCompleto,
                    //    nombreLicitacion: model.nombreLicitacion,
                    //    numeroGarantia: model.numeroGarantia
                    //);

                    //await _emailService.SendEmailAsync(
                    //    toEmail: usuario.Email,
                    //    toName: usuario.nombreCompleto,
                    //    subject: $"Confirmación de Garantía - {usuario.nombreCompleto}",
                    //    htmlBody: htmlEmpleado
                    //);

                    //// 2. Correo de notificación al buzón de revision para garantias
                    //var htmlRevision = PlantillasEmail.NotificacionRevisionGarantia(
                    //    nombreEmpleado: usuario.nombreCompleto,
                    //    nombreLicitacion: model.nombreLicitacion
                    //);

                    //await _emailService.SendEmailAsync(
                    //    toEmail: "Garantias-Financiero@sistemasanaliticos.cr",
                    //    toName: "Garantías Financiero",
                    //    subject: $"Solicitud de Garantía Revisión - {usuario.nombreCompleto}",
                    //    htmlBody: htmlRevision
                    //);
                }
                catch (Exception exEmail)
                {
                    // Solo log si hay error en correo, no interrumpir
                    Console.WriteLine($"Error enviando correo: {exEmail.Message}");
                }

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = DateOnly.FromDateTime(ahoraCR),
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Garantia",
                    Accion = "Nuevo Registro"
                };
                _context.Auditoria.Add(auditoria);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageGarantia"] = "Se creó la garantía correctamente.";
                return RedirectToAction("VerGarantias");
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error en creación de garantía: {ex.Message}");
                TempData["ErrorMessageGarantia"] = "Error en la creación de la garantía.";
                return RedirectToAction("VerGarantias");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CAMBIOS DE ESTADOS MASIVOS
        [Authorize(Policy = "Garantia.CambiarEstado")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMasivo([FromBody] EstadoMasivoViewModel model)
        {
            // Auditoría
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"
                : "America/Costa_Rica";

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
            var usuario = await _userManager.GetUserAsync(User);

            foreach (var id in model.Ids)
            {
                var garantia = await _context.Garantia.FindAsync(id);
                if (garantia == null) continue; // Agregado: si no existe, salta

                var usuarioGarantia = await _userManager.FindByIdAsync(garantia.UsuarioId);
                if (usuarioGarantia == null) continue; // Agregado: validación básica

                try
                {
                    if (garantia.estado == "Aprobada")
                    {
                        var auditoriaApro = new Auditoria
                        {
                            Fecha = hoy,
                            Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                            Usuario = usuario.nombreCompleto ?? "Desconocido",
                            Tabla = "Garantia",
                            Accion = "Intento de Cambio de Garantía aprobada no." + id + " de " + usuarioGarantia.nombreCompleto
                        };
                        _context.Auditoria.Add(auditoriaApro);

                        TempData["ErrorMessageGarantia"] = $"La garantía {id} de {usuarioGarantia.nombreCompleto} no cambió de estado porque ya tenia estado anteriormente.";
                    }
                    else if (garantia.estado == "Rechazada")
                    {
                        var auditoriaRecha = new Auditoria
                        {
                            Fecha = hoy,
                            Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                            Usuario = usuario.nombreCompleto ?? "Desconocido",
                            Tabla = "Garantia",
                            Accion = "Intento de Cambio de Garantía rechazada no." + id + " de " + usuarioGarantia.nombreCompleto
                        };
                        _context.Auditoria.Add(auditoriaRecha);

                        TempData["ErrorMessageGarantia"] = $"La garantía {id} de {usuarioGarantia.nombreCompleto} no cambió de estado porque ya tenia estado anteriormente.";
                    }
                    else if (model.estado == "Aprobada")
                    {
                        // ✅ GUARDAR CAMBIOS DE LA GARANTÍA (solo si se aprobó)
                        garantia.estado = "Aprobada";
                        _context.Garantia.Update(garantia);
                        await _context.SaveChangesAsync();

                        // Enviar correo de aprobación
                        try
                        {
                            //var htmlEmpleado = PlantillasEmail.EstadoGarantiaAprob(
                            //    nombreEmpleado: usuario.nombreCompleto,
                            //    nombreLicitacion: garantia.nombreLicitacion,
                            //    numeroGarantia: garantia.numeroGarantia,
                            //    tipoGarantia: garantia.tipoLicitacion
                            //);
                            //await _emailService.SendEmailAsync(
                            //    toEmail: usuarioGarantia.Email,
                            //    toName: usuarioGarantia.nombreCompleto,
                            //    subject: $"Aprobación de Garantía - {usuarioGarantia.nombreCompleto}",
                            //    htmlBody: htmlEmpleado
                            //);
                        }
                        catch (Exception exEmail)
                        {
                            Console.WriteLine($"Error enviando correo para permiso {id}: {exEmail.Message}");
                        }
                    }
                    else if (model.estado == "Rechazada")
                    {
                        garantia.estado = "Rechazada";
                        _context.Garantia.Update(garantia);
                        await _context.SaveChangesAsync();

                        // Enviar correo de rechazo
                        try
                        {
                            //var htmlEmpleado = PlantillasEmail.EstadoGarantiaRechaz(
                            //    nombreEmpleado: usuario.nombreCompleto,
                            //    nombreLicitacion: garantia.nombreLicitacion,
                            //    numeroGarantia: garantia.numeroGarantia,
                            //    tipoGarantia: garantia.tipoLicitacion
                            //);
                            //await _emailService.SendEmailAsync(
                            //    toEmail: usuarioGarantia.Email,
                            //    toName: usuarioGarantia.nombreCompleto,
                            //    subject: $"Rechazo de Garantía - {usuarioGarantia.nombreCompleto}",
                            //    htmlBody: htmlEmpleado
                            //);
                        }
                        catch (Exception exEmail)
                        {
                            Console.WriteLine($"Error enviando correo para permiso {id}: {exEmail.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando permiso {id}: {ex.Message}");
                    // Opcional: agregar a TempData un mensaje de error general
                }
            }

            var auditoria = new Auditoria
            {
                Fecha = hoy,
                Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                Usuario = usuario.nombreCompleto ?? "Desconocido",
                Tabla = "Garantia",
                Accion = "Cambio de Estado del no." + string.Join(", ", model.Ids)
            };
            _context.Auditoria.Add(auditoria);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageGarantia"] = "Se cambiaron los permisos de estados correctamente.";
            return Ok(new { redirect = Url.Action("VerGarantias") });
        }
    }
}
