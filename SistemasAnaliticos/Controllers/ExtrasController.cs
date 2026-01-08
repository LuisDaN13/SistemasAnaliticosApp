using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.ViewModels;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class ExtrasController : Controller
    {

        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IPermisoAlcanceService _permisoAlcanceService;

        public ExtrasController(DBContext context, UserManager<Usuario> userManager, IPermisoAlcanceService permisoAlcanceService)
        {
            _context = context;
            _userManager = userManager;
            _permisoAlcanceService = permisoAlcanceService;
        }

        // --------------------------------------------------------------------------------------
        // CREATE DE LA CABECERA DE LA EXTRA
        [HttpGet]
        [Authorize(Policy = "Extra.Ver")]
        public async Task<IActionResult> VerExtras(int page = 1)
        {
            // Eliminar cabeceras en estado "Creada" que no tengan detalles asociados
            var cabecerasSinDetalles = await _context.Extras
                .Include(l => l.Detalles)
                .Where(l => l.estado == "Creada" && !l.Detalles.Any())
                .ToListAsync();

            if (cabecerasSinDetalles.Any())
            {
                _context.Extras.RemoveRange(cabecerasSinDetalles);
                await _context.SaveChangesAsync();
            }

            int pageSize = 3;

            var query = _context.Extras.AsNoTracking();
            query = await _permisoAlcanceService.AplicarAlcanceExtrasConTipoAsync(query, User);

            var totalExtras = await query.CountAsync();

            var extras = await query
                .AsNoTracking()
                .Include(lv => lv.Detalles)
                .OrderByDescending(x => x.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(lv => new ExtraViewModel
                {
                    idExtra = lv.idExtra,
                    fechaCreacion = lv.fechaCreacion,
                    nombreEmpleado = lv.nombreEmpleado,
                    departamento = lv.departamento,
                    jefe = lv.jefe,
                    total = lv.totalHoras,
                    cantidadDetalle = lv.Detalles.Count,
                    lugares = lv.Detalles.Any()
                    ? string.Join(", ", lv.Detalles.Select(d => d.lugar).Distinct().Take(5))
                    : "Sin lugares",
                    estado = lv.estado,

                    tipoExtra = lv.tipoExtra
                })
                .ToListAsync();

            var viewModel = new PaginacionExtraViewModel
            {
                Extras = extras,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalExtras / (double)pageSize)
            };

            return View(viewModel);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA TENER TODOS LOS VIATICOS (sin paginación)
        public async Task<IActionResult> ObtenerTodasLasExtras()
        {
            try
            {
                var query = _context.Extras.AsNoTracking();
                query = await _permisoAlcanceService.AplicarAlcanceExtrasConTipoAsync(query, User);

                var todasLasExtras = await query
                    .AsNoTracking()
                    .Include(lv => lv.Detalles)
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(lv => new
                    {
                        lv.idExtra,
                        lv.fechaCreacion,
                        lv.nombreEmpleado,
                        lv.departamento,
                        lv.jefe,
                        lv.totalHoras,
                        lv.estado,
                        cantidadDetalle = lv.Detalles.Count,
                        lugares = lv.Detalles.Any()
                            ? string.Join(", ", lv.Detalles.Select(d => d.lugar).Distinct().Take(5))
                            : "Sin lugares"
                    })
                    .ToListAsync();

                return Ok(todasLasExtras);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // METODO PARA OBTENER VIÁTICOS FILTRADOS COMPLETOS
        [HttpGet]
        public async Task<IActionResult> ObtenerExtrasFiltradosCompletos(
            [FromQuery] string[] lugar,
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
                var query = _context.Extras.AsNoTracking().Include(v => v.Detalles).AsQueryable();
                query = await _permisoAlcanceService.AplicarAlcanceExtrasConTipoAsync(query, User);

                // Aplicar filtros de lugar
                if (lugar != null && lugar.Length > 0)
                {
                    // Filtrar viáticos que tengan al menos un detalle con el tipo especificado
                    query = query.Where(v => v.Detalles.Any(d => lugar.Contains(d.lugar)));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    // Mapear estados del frontend al backend
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobada" ? "Aprobada" :
                        e == "Creada" ? "Creada" :
                        e == "Rechazado" ? "Rechazada" :
                        e 
                    ).ToArray();

                    query = query.Where(v => estadosMapeados.Contains(v.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(v => departamentos.Contains(v.departamento));
                }

                // Aplicar filtro por nombre
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    query = query.Where(p =>
                        (!string.IsNullOrEmpty(p.nombreEmpleado) && p.nombreEmpleado.Contains(nombre)) ||
                        p.Detalles.Any(d => !string.IsNullOrEmpty(d.sucursal) && d.sucursal.Contains(nombre))
                    );
                }

                // Aplicar filtros de fecha
                if (!string.IsNullOrEmpty(fechaTipo))
                {
                    if (fechaTipo == "single" && !string.IsNullOrEmpty(fechaUnica))
                    {
                        if (DateOnly.TryParse(fechaUnica, out DateOnly fechaUnicaDate))
                        {
                            query = query.Where(v => v.fechaCreacion == fechaUnicaDate);
                        }
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(v =>
                                v.fechaCreacion >= desdeDate &&
                                v.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var extras = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(v => new
                    {
                        v.idExtra,
                        v.nombreEmpleado,
                        v.departamento,
                        v.jefe,
                        v.totalHoras,
                        v.estado,
                        v.fechaCreacion,
                        cantidadDetalle = v.Detalles.Count,
                        lugares = v.Detalles.Any()
                            ? string.Join(", ", v.Detalles.Select(d => d.lugar).Distinct())
                            : "Sin lugares",
                        detalles = v.Detalles.Select(d => new
                        {
                            d.idExtrasDetalle,
                            d.fecha,
                            d.horaInicio,
                            d.horaFin,
                            d.detalle,
                            d.lugar,
                            d.sucursal,
                            d.atm,
                            d.noCaso,
                            d.noBoleta
                        }).ToList()                        
                    })
                    .ToListAsync();

                return Ok(extras);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor al aplicar filtros");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR VIÁTICOS FILTRADOS EN EXCEL
        public async Task<IActionResult> ExportarExtrasExcel(
            [FromQuery] string[] tiposDetalle,
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
                var query = _context.LiquidacionViatico.AsNoTracking().Include(v => v.Detalles).AsQueryable();
                query = await _permisoAlcanceService.AplicarAlcanceViaticoAsync(query, User);

                // Aplicar filtros de tipo de detalle
                if (tiposDetalle != null && tiposDetalle.Length > 0)
                {
                    query = query.Where(v => v.Detalles.Any(d => tiposDetalle.Contains(d.tipo)));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    // Mapear estados para coincidir con el backend
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobado" ? "Aprobada" :
                        e == "Rechazado" ? "Rechazada" :
                        e  // Mantener otros estados como están
                    ).ToArray();

                    query = query.Where(v => estadosMapeados.Contains(v.estado));
                }

                // Aplicar filtros de departamento
                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(v => departamentos.Contains(v.departamento));
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
                        query = query.Where(v => v.fechaCreacion == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(v =>
                                v.fechaCreacion >= desdeDate &&
                                v.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var viaticos = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(v => new
                    {
                        v.idViatico,
                        v.nombreEmpleado,
                        v.departamento,
                        v.jefe,
                        v.total,
                        v.estado,
                        v.fechaCreacion,
                        // Propiedades de detalles
                        Detalles = v.Detalles.Select(d => new
                        {
                            d.fecha,
                            d.tipo,
                            d.monto,
                            d.detalle
                        }).ToList(),
                        CantidadDetalles = v.Detalles.Count,
                        lugares = v.Detalles.Any()
                            ? string.Join(", ", v.Detalles.Select(d => d.tipo).Distinct())
                            : "Sin detalles"
                    })
                    .ToListAsync();

                // Crear el libro de Excel
                using var workbook = new ClosedXML.Excel.XLWorkbook();

                // Hoja principal - Resumen de viáticos
                var worksheet = workbook.Worksheets.Add("Viáticos");

                // Estilos para el encabezado
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Encabezados principales
                worksheet.Cell(1, 1).Value = "ID Viático";
                worksheet.Cell(1, 2).Value = "Nombre Empleado";
                worksheet.Cell(1, 3).Value = "Departamento";
                worksheet.Cell(1, 4).Value = "Jefe";
                worksheet.Cell(1, 5).Value = "Total";
                worksheet.Cell(1, 6).Value = "Estado";
                worksheet.Cell(1, 7).Value = "Fecha Creación";
                worksheet.Cell(1, 8).Value = "Cant. Detalles";
                worksheet.Cell(1, 9).Value = "Tipos de Detalles";

                // Aplicar estilo al encabezado
                worksheet.Range(1, 1, 1, 9).Style = headerStyle;

                // Llenar datos principales
                int row = 2;
                foreach (var viatico in viaticos)
                {
                    worksheet.Cell(row, 1).Value = viatico.idViatico;
                    worksheet.Cell(row, 2).Value = viatico.nombreEmpleado;
                    worksheet.Cell(row, 3).Value = viatico.departamento;
                    worksheet.Cell(row, 4).Value = viatico.jefe;
                    worksheet.Cell(row, 5).Value = viatico.total;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 6).Value = viatico.estado;
                    worksheet.Cell(row, 7).Value = viatico.fechaCreacion.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 8).Value = viatico.CantidadDetalles;
                    //worksheet.Cell(row, 9).Value = viatico.TiposDetalles;

                    row++;
                }

                // Hoja de detalles
                if (viaticos.Any(v => v.Detalles.Any()))
                {
                    var detallesSheet = workbook.Worksheets.Add("Detalles");

                    // Encabezados de detalles
                    detallesSheet.Cell(1, 1).Value = "ID Viático";
                    detallesSheet.Cell(1, 2).Value = "Empleado";
                    detallesSheet.Cell(1, 3).Value = "Fecha Detalle";
                    detallesSheet.Cell(1, 4).Value = "Tipo";
                    detallesSheet.Cell(1, 5).Value = "Monto";
                    detallesSheet.Cell(1, 6).Value = "Detalle";
                    detallesSheet.Range(1, 1, 1, 6).Style = headerStyle;

                    int detalleRow = 2;
                    foreach (var viatico in viaticos)
                    {
                        foreach (var detalle in viatico.Detalles)
                        {
                            detallesSheet.Cell(detalleRow, 1).Value = viatico.idViatico;
                            detallesSheet.Cell(detalleRow, 2).Value = viatico.nombreEmpleado;
                            detallesSheet.Cell(detalleRow, 3).Value = detalle.fecha.ToString("yyyy-MM-dd");
                            detallesSheet.Cell(detalleRow, 4).Value = detalle.tipo;
                            detallesSheet.Cell(detalleRow, 5).Value = detalle.monto;
                            detallesSheet.Cell(detalleRow, 5).Style.NumberFormat.Format = "#,##0.00";
                            detallesSheet.Cell(detalleRow, 6).Value = detalle.detalle;
                            detalleRow++;
                        }
                    }
                    detallesSheet.Columns().AdjustToContents();
                }                

                // Ajustar ancho de columnas en la hoja principal
                worksheet.Columns().AdjustToContents();

                // Preparar respuesta
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Viaticos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

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
                    Tabla = "Viatico",
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

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR VIÁTICOS FILTRADOS EN PDF
        public async Task<IActionResult> ExportarViaticosPDF(
            [FromQuery] string[] tiposDetalle,
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
                var query = _context.LiquidacionViatico.AsNoTracking().Include(v => v.Detalles).AsQueryable();
                query = await _permisoAlcanceService.AplicarAlcanceViaticoAsync(query, User);

                // Aplicar filtros (misma lógica que Excel)
                if (tiposDetalle != null && tiposDetalle.Length > 0)
                {
                    query = query.Where(v => v.Detalles.Any(d => tiposDetalle.Contains(d.tipo)));
                }

                if (estados != null && estados.Length > 0)
                {
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobado" ? "Aprobada" :
                        e == "Rechazado" ? "Rechazada" :
                        e).ToArray();

                    query = query.Where(v => estadosMapeados.Contains(v.estado));
                }

                if (departamentos != null && departamentos.Length > 0)
                {
                    query = query.Where(v => departamentos.Contains(v.departamento));
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
                        query = query.Where(v => v.fechaCreacion == fechaUnicaDate);
                    }
                    else if (fechaTipo == "range" && !string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
                    {
                        if (DateOnly.TryParse(fechaDesde, out DateOnly desdeDate) &&
                            DateOnly.TryParse(fechaHasta, out DateOnly hastaDate))
                        {
                            query = query.Where(v =>
                                v.fechaCreacion >= desdeDate &&
                                v.fechaCreacion <= hastaDate
                            );
                        }
                    }
                }

                var viaticos = await query
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(v => new
                    {
                        v.idViatico,
                        v.nombreEmpleado,
                        v.departamento,
                        v.jefe,
                        v.total,
                        v.estado,
                        v.fechaCreacion,
                        Detalles = v.Detalles.Select(d => new
                        {
                            d.fecha,
                            d.tipo,
                            d.monto,
                            d.detalle
                        }).ToList(),
                        CantidadDetalles = v.Detalles.Count,
                        TiposDetalles = v.Detalles.Any()
                            ? string.Join(", ", v.Detalles.Select(d => d.tipo).Distinct())
                            : "Sin detalles"
                    })
                    .ToListAsync();

                // Crear PDF
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Título
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var titulo = new Paragraph("REPORTE DE LIQUIDACIONES DE VIÁTICOS", tituloFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 20f;
                document.Add(titulo);

                // Fecha de generación
                var fechaFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC);
                var fechaGeneracion = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fechaFont);
                fechaGeneracion.Alignment = Element.ALIGN_RIGHT;
                fechaGeneracion.SpacingAfter = 10f;
                document.Add(fechaGeneracion);

                // Información del reporte
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var infoText = new Paragraph($"Total de viáticos: {viaticos.Count}\n" +
                                            $"Monto total: {viaticos.Sum(v => v.total):C2}\n" +
                                            $"Fecha del reporte: {DateTime.Now:dd/MM/yyyy}", infoFont);
                infoText.SpacingAfter = 15f;
                document.Add(infoText);

                // Crear tabla principal
                var table = new PdfPTable(7);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.8f, 1.5f, 1.5f, 1.5f, 1.2f, 1.2f, 1.5f });

                // Encabezados de tabla
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                AddTableCell(table, "ID", headerFont);
                AddTableCell(table, "Empleado", headerFont);
                AddTableCell(table, "Departamento", headerFont);
                AddTableCell(table, "Jefe", headerFont);
                AddTableCell(table, "Total", headerFont);
                AddTableCell(table, "Estado", headerFont);
                AddTableCell(table, "Fecha", headerFont);

                // Datos
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                var currencyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Blue);

                foreach (var viatico in viaticos)
                {
                    AddTableCell(table, viatico.idViatico.ToString(), cellFontBold);
                    AddTableCell(table, viatico.nombreEmpleado ?? "", cellFont);
                    AddTableCell(table, viatico.departamento ?? "", cellFont);
                    AddTableCell(table, viatico.jefe ?? "", cellFont);

                    // Celda de total con formato de moneda
                    var totalCell = new PdfPCell(new Phrase($"₡{viatico.total:N2}", currencyFont));
                    totalCell.Padding = 5;
                    totalCell.BorderWidth = 0.5f;
                    totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(totalCell);

                    // Estado con color
                    var estadoFont = GetEstadoFontViaticos(viatico.estado);
                    AddTableCell(table, viatico.estado ?? "", estadoFont);

                    AddTableCell(table, viatico.fechaCreacion.ToString("dd/MM/yyyy"), cellFont);

                    // Si tiene detalles, agregar fila adicional con información
                    if (viatico.CantidadDetalles > 0)
                    {
                        var detallesCell = new PdfPCell(new Phrase(
                            $"Detalles: {viatico.CantidadDetalles} | Tipos: {viatico.TiposDetalles}",
                            FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.DarkGray)));
                        detallesCell.Colspan = 7;
                        detallesCell.Padding = 3;
                        detallesCell.BorderWidth = 0.5f;
                        table.AddCell(detallesCell);
                    }
                }

                document.Add(table);

                // Sección de detalles si hay viáticos con detalles
                if (viaticos.Any(v => v.Detalles.Any()))
                {
                    document.Add(new Paragraph("\n\n"));

                    var detallesTitulo = new Paragraph("DETALLES DE VIÁTICOS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                    detallesTitulo.Alignment = Element.ALIGN_CENTER;
                    detallesTitulo.SpacingAfter = 10f;
                    document.Add(detallesTitulo);

                    foreach (var viatico in viaticos.Where(v => v.Detalles.Any()))
                    {
                        var viaticoHeader = new Paragraph(
                            $"Viático #{viatico.idViatico} - {viatico.nombreEmpleado} (Total: ₡{viatico.total:N2})",
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9));
                        viaticoHeader.SpacingAfter = 5f;
                        document.Add(viaticoHeader);

                        var detallesTable = new PdfPTable(4);
                        detallesTable.WidthPercentage = 90;
                        detallesTable.SetWidths(new float[] { 1f, 1f, 1f, 3f });

                        // Encabezados de detalles
                        var detalleHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                        AddTableCell(detallesTable, "Fecha", detalleHeaderFont);
                        AddTableCell(detallesTable, "Tipo", detalleHeaderFont);
                        AddTableCell(detallesTable, "Monto", detalleHeaderFont);
                        AddTableCell(detallesTable, "Detalle", detalleHeaderFont);

                        // Datos de detalles
                        var detalleFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                        foreach (var detalle in viatico.Detalles)
                        {
                            AddTableCell(detallesTable, detalle.fecha.ToString("dd/MM/yyyy"), detalleFont);
                            AddTableCell(detallesTable, detalle.tipo ?? "", detalleFont);

                            var montoCell = new PdfPCell(new Phrase($"₡{detalle.monto:N2}", detalleFont));
                            montoCell.Padding = 5;
                            montoCell.BorderWidth = 0.5f;
                            montoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                            detallesTable.AddCell(montoCell);

                            AddTableCell(detallesTable, detalle.detalle ?? "", detalleFont);
                        }

                        document.Add(detallesTable);
                        document.Add(new Paragraph("\n"));
                    }
                }

                // Pie de página
                var pieFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC);
                var pie = new Paragraph($"Reporte generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm:ss}", pieFont);
                pie.Alignment = Element.ALIGN_CENTER;
                pie.SpacingBefore = 20f;
                document.Add(pie);

                document.Close();

                // Devolver PDF
                var fileName = $"Viaticos_{DateTime.Now:yyyyMMddHHmmss}.pdf";

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
                    Tabla = "Viatico",
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

        // -------------------------------------------------------------------------------------------------------------------------------
        // MÉTODOS AUXILIARES PARA PDF
        private void AddTableCell(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 5;
            cell.BorderWidth = 0.5f;
            table.AddCell(cell);
        }
        private Font GetEstadoFontViaticos(string estado)
        {
            var baseFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            if (string.IsNullOrEmpty(estado)) return baseFont;

            return estado.ToUpper() switch
            {
                "APROBADA" or "CREADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Green),
                "PENDIENTE" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Orange),
                "RECHAZADA" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Red),
                "LIQUIDADO" => FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Blue),
                _ => baseFont
            };
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA OBTENER CONTADORES DE FILTROS (INCLUYENDO TIPOS DE DETALLES)
        [HttpGet]
        public async Task<IActionResult> ObtenerContadoresFiltros()
        {
            try
            {
                var baseQuery = _context.Extras
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                    .AsQueryable();

                baseQuery = await _permisoAlcanceService.AplicarAlcanceExtrasConTipoAsync(baseQuery, User);

                var data = await baseQuery
                    .Select(v => new
                    {
                        v.idExtra,
                        v.departamento,
                        v.estado,
                        v.totalHoras,
                        lugares = v.Detalles.Select(d => d.lugar).ToList(),
                        CantidadDetalles = v.Detalles.Count
                    })
                    .ToListAsync();

                // Definir los tipos de detalles que quieres contar
                var lugares = new List<string>
                    {
                        "BN",
                        "BCR",
                        "BCT",
                        "Global Exchange",
                        "Mutual Alajuela",
                        "SASA"
                    };

                // Calcular contadores
                var contadores = new
                {
                    // Contadores por tipo de detalle
                    BN = data.Count(v => v.lugares.Any(t => t == "BN")),
                    BCR = data.Count(v => v.lugares.Any(t => t == "BCR")),
                    BCT = data.Count(v => v.lugares.Any(t => t == "BCT")),
                    Global = data.Count(v => v.lugares.Any(t => t == "Global Exchange")),
                    Mutual = data.Count(v => v.lugares.Any(t => t == "Mutual Alajuela")),
                    SASA = data.Count(v => v.lugares.Any(t => t == "SASA")),

                    // Contadores por estado
                    Creada = data.Count(v => v.estado == "Creada"),
                    Aprobada = data.Count(v => v.estado == "Aprobada"),
                    Rechazada = data.Count(v => v.estado == "Rechazada"),

                    // Contadores por departamento
                    Bodega = data.Count(v => v.departamento == "Bodega"),
                    FinancieroContable = data.Count(v => v.departamento == "Financiero Contable"),
                    Gerencia = data.Count(v => v.departamento == "Gerencia"),
                    Ingenieria = data.Count(v => v.departamento == "Ingeniería"),
                    Jefatura = data.Count(v => v.departamento == "Jefatura"),
                    Legal = data.Count(v => v.departamento == "Legal"),
                    Operaciones = data.Count(v => v.departamento == "Operaciones"),
                    RecursosHumanos = data.Count(v => v.departamento == "Recursos Humanos"),
                    TecnicosNCR = data.Count(v => v.departamento == "Tecnicos NCR"),
                    TecnologiasInformacion = data.Count(v => v.departamento == "Tecnologías de Información"),
                    Ventas = data.Count(v => v.departamento == "Ventas"),
                };

                return Ok(contadores);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    // Contadores por tipo de detalle
                    BN = 0,
                    BCR = 0,
                    BCT = 0,
                    Global = 0,
                    Mutual = 0,
                    SASA = 0,

                    // Contadores por estado
                    Creada = 0,
                    Aprobada = 0,
                    Rechazada = 0,

                    // Contadores por departamento
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
                    Ventas = 0,
                });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CAMBIOS DE ESTADOS MASIVOS
        [Authorize(Policy = "Extra.CambiarEstado")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMasivo([FromBody] EstadoMasivoViewModel model)
        {
            foreach (var id in model.Ids)
            {
                var extra = await _context.Extras.FindAsync(id);
                if (extra != null)
                {
                    extra.estado = model.estado;
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
                Tabla = "Extra",
                Accion = "Cambio de Estado del no." + string.Join(", ", model.Ids)
            };
            _context.Auditoria.Add(auditoria);

            TempData["SuccessMessageEx"] = "Se cambiaron las extras de estados correctamente.";
            return Ok(new { redirect = Url.Action("VerExtras") });
        }

        // --------------------------------------------------------------------------------------
        // CREATE DE LA CABECERA DE LA EXTRA
        [Authorize(Policy = "Extra.Crear")]
        [HttpGet]
        public async Task<IActionResult> CreateExtra(string tipo, string tecnicoId)
        {
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"
                : "America/Costa_Rica";

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);

            var usuario = tipo == "Técnicos NCR" && !string.IsNullOrEmpty(tecnicoId)
                ? await _userManager.FindByIdAsync(tecnicoId)
                : await _userManager.GetUserAsync(User);

            var tipoExtra = "Empleado";

            if (tipo == "Técnicos NCR" && !string.IsNullOrEmpty(tecnicoId))
            {
                usuario = await _userManager.FindByIdAsync(tecnicoId);
                if (usuario == null)
                    return NotFound("Técnico no encontrado");

                tipoExtra = "Técnico";
            }
            else
            {
                usuario = await _userManager.GetUserAsync(User);
            }

            var extra = new Extras
            {
                UsuarioId = usuario.Id,
                fechaCreacion = hoy,
                tipoExtra = tipoExtra,
                nombreEmpleado = usuario.nombreCompleto ?? "Sin nombre",
                departamento = usuario.departamento ?? "Sin departamento",
                jefe = usuario.jefeNombre ?? "Sin jefe",
                estado = "Creada",
                totalHoras = 0
            };

            _context.Add(extra);

            //var auditoria = new Auditoria
            //{
            //    Fecha = hoy,
            //    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
            //    Usuario = usuario.nombreCompleto ?? "Desconocido",
            //    Tabla = "Extra",
            //    Accion = "Nuevo registro"
            //};

            //_context.Auditoria.Add(auditoria);

            await _context.SaveChangesAsync();

            return RedirectToAction("EditExtra", new { id = extra.idExtra });
        }

        // --------------------------------------------------------------------------------------
        // MUESTRA LA VISTA DE EDIT CON CABECERA Y DETALLES
        [Authorize(Policy = "Extra.Crear")]
        [HttpGet]
        public async Task<IActionResult> EditExtra(long id)
        {
            var Extra = await _context.Extras
                    .Include(x => x.Detalles)
                    .FirstOrDefaultAsync(x => x.idExtra == id);

            if (Extra == null) return NotFound();

            return View(Extra);
        }

        // --------------------------------------------------------------------------------------
        // AGREGAR DETALLE A EXTRAS
        [Authorize(Policy = "Extra.Crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarExtrasDetalle([FromForm] AgregarDetalleExtraViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessageEx"] = "Error al validar el detalle. Revise los campos.";
                return RedirectToAction(nameof(EditExtra), new { id = model.idExtra });
            }

            var detalle = new ExtrasDetalle
            {
                idExtra = model.idExtra,
                fecha = model.fecha,
                horaInicio = model.horaInicio,
                horaFin = model.horaFin,
                detalle = model.detalle,
                atm = model.atm,
                noCaso = model.noCaso,
                noBoleta = model.noBoleta,
                lugar = model.lugar,
                sucursal = model.sucursal
            };

            _context.ExtrasDetalle.Add(detalle);
            await _context.SaveChangesAsync();
            await RecalcularTotalHorasExtra(model.idExtra);

            TempData["SuccessMessageEx"] = "Detalle agregado correctamente.";
            return RedirectToAction(nameof(EditExtra), new { id = model.idExtra });
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // VER DETALLES DE VIÁTICOS
        [Authorize(Policy = "Extra.Detalles")]
        [HttpGet]
        [Route("Extras/DetailsExtras/{id}")]
        public async Task<ActionResult> DetailsExtras(long id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessageEx"] = "No se proporcionó un identificador de una extra válida.";
                return RedirectToAction("VerExtras", "Extras");
            }

            // Buscar el viático con sus detalles
            var extra = await _context.Extras
                .Include(v => v.Detalles)  // Incluir los detalles relacionados
                .FirstOrDefaultAsync(v => v.idExtra == id);

            if (extra == null)
            {
                TempData["ErrorMessageEx"] = "La extra que intentas ver no existe o fue eliminado.";
                return RedirectToAction("VerExtras", "Extras");
            }

            // Crear ViewModel para mostrar datos formateados
            var viewModel = new ExtraDetailsViewModel
            {
                idExtra = extra.idExtra,
                fechaCreacion = extra.fechaCreacion,
                nombreEmpleado = extra.nombreEmpleado,
                departamento = extra.departamento,
                jefe = extra.jefe,
                totalHoras = extra.totalHoras,
                estado = extra.estado,

                // Detalles formateados
                Detalles = extra.Detalles.Select(d => new DetalleExtraViewModel
                {
                    idExtraDetalle = d.idExtrasDetalle,
                    fecha = d.fecha,
                    horaInicio = d.horaInicio,
                    horaFin = d.horaFin,
                    detalle = d.detalle,
                    atm = d.atm,
                    noBoleta = d.noBoleta,
                    noCaso = d.noCaso,
                    lugar = d.lugar,
                    sucursal = d.sucursal
                }).ToList(),

                cantidadDetalles = extra.Detalles.Count,
                lugares = extra.Detalles.Any()
                    ? string.Join(", ", extra.Detalles.Select(d => d.lugar).Distinct())
                    : "Sin detalles",
            };

            return View(viewModel);
        }

        // --------------------------------------------------------------------------------------
        // ELIMINAR DETALLE DE EXTRAS
        [Authorize(Policy = "Extra.Crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDetalle(long idDetalle, long idExtra)
        {
            var detalle = await _context.ExtrasDetalle.FindAsync(idDetalle);
            if (detalle == null)
            {
                TempData["ErrorMessageEx"] = "Detalle no encontrado.";
                return RedirectToAction(nameof(EditExtra), new { id = idExtra });
            }

            _context.ExtrasDetalle.Remove(detalle);
            await _context.SaveChangesAsync();
            await RecalcularTotalHorasExtra(idExtra);

            TempData["SuccessMessageEx"] = "Detalle eliminado correctamente.";
            return RedirectToAction(nameof(EditExtra), new { id = idExtra });
        }


        // --------------------------------------------------------------------------------------
        // RECALCULAR TOTAL DE HORAS EXTRA
        [Authorize(Policy = "Extra.Crear")]
        private async Task RecalcularTotalHorasExtra(long idExtra)
        {
            var extra = await _context.Extras
                                      .Include(e => e.Detalles)
                                      .FirstOrDefaultAsync(e => e.idExtra == idExtra);

            if (extra == null) return;

            decimal totalHoras = 0;

            foreach (var d in extra.Detalles)
            {
                if (d.horaInicio.HasValue && d.horaFin.HasValue)
                {
                    var horas = (d.horaFin.Value - d.horaInicio.Value).TotalHours;

                    if (horas > 0)
                    {
                        totalHoras += (decimal)horas;
                    }
                }
            }

            // Redondear a 1 decimal
            extra.totalHoras = Math.Round(totalHoras, 2);

            _context.Update(extra);
            await _context.SaveChangesAsync();
        }
    }
}
