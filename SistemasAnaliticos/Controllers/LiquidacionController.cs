using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class LiquidacionController : Controller
    {

        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;

        public LiquidacionController(DBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --------------------------------------------------------------------------------------
        // CREATE DE LA CABECERA DE LA LIQUIDACION
        [HttpGet]
        public async Task<IActionResult> VerViaticos(int page = 1)
        {
            // Buscar liquidaciones creadas sin detalles
            var cabecerasSinDetalles = await _context.LiquidacionViatico
                .Include(l => l.Detalles)
                .Where(l => l.estado == "Creada" && !l.Detalles.Any())
                .ToListAsync();

            if (cabecerasSinDetalles.Any())
            {
                _context.LiquidacionViatico.RemoveRange(cabecerasSinDetalles);
                await _context.SaveChangesAsync();
            }

            int pageSize = 3;

            var totalViaticos = await _context.LiquidacionViatico
                .CountAsync();

            var viaticos = await _context.LiquidacionViatico
                .AsNoTracking()
                .Include(lv => lv.Detalles)
                .OrderByDescending(x => x.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(lv => new LiquidacionViaticoViewModel
                {
                    idViatico = lv.idViatico,
                    fechaCreacion = lv.fechaCreacion,
                    nombreEmpleado = lv.nombreEmpleado,
                    departamento = lv.departamento,
                    jefe = lv.jefe,
                    total = lv.total,
                    cantidadDetalle = lv.Detalles.Count,
                    tipoDetalles = lv.Detalles.Any()
                    ? string.Join(", ", lv.Detalles.Select(d => d.tipo).Distinct().Take(5))
                    : "Sin detalles",
                    estado = lv.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionLiquidacionViaticoViewModel
            {
                LiquidacionesViaticos = viaticos,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalViaticos / (double)pageSize)
            };

            return View(viewModel);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ENDPOINT PARA TENER TODOS LOS VIATICOS (sin paginación)
        public async Task<IActionResult> ObtenerTodosLosViaticos()
        {
            try
            {
                var todosLosViaticos = await _context.LiquidacionViatico
                    .AsNoTracking()
                    .Include(lv => lv.Detalles)
                    .OrderByDescending(x => x.fechaCreacion)
                    .Select(lv => new
                    {
                        lv.idViatico,
                        lv.fechaCreacion,
                        lv.nombreEmpleado,
                        lv.departamento,
                        lv.jefe,
                        lv.total,
                        lv.estado,
                        cantidadDetalle = lv.Detalles.Count,
                        tipoDetalles = lv.Detalles.Any()
                            ? string.Join(", ", lv.Detalles.Select(d => d.tipo).Distinct().Take(5))
                            : "Sin detalles"
                    })
                    .ToListAsync();

                return Ok(todosLosViaticos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // METODO PARA OBTENER VIÁTICOS FILTRADOS COMPLETOS
        [HttpGet]
        public async Task<IActionResult> ObtenerViaticosFiltradosCompletos(
            [FromQuery] string[] tiposDetalle,
            [FromQuery] string[] estados,
            [FromQuery] string[] departamentos,
            [FromQuery] string fechaTipo = null,
            [FromQuery] string fechaUnica = null,
            [FromQuery] string fechaDesde = null,
            [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.LiquidacionViatico
                    .AsNoTracking()
                    .Include(v => v.Detalles)  // Incluir detalles para filtrar por tipoDetalle
                    .AsQueryable();

                // Aplicar filtros de tipo de detalle
                if (tiposDetalle != null && tiposDetalle.Length > 0)
                {
                    // Filtrar viáticos que tengan al menos un detalle con el tipo especificado
                    query = query.Where(v => v.Detalles.Any(d => tiposDetalle.Contains(d.tipo)));
                }

                // Aplicar filtros de estado
                if (estados != null && estados.Length > 0)
                {
                    // Mapear estados del frontend al backend
                    var estadosMapeados = estados.Select(e =>
                        e == "Aprobada" ? "Aprobada" :
                        e == "Creada" ? "Creada" :
                        e == "Pendiente" ? "Pendiente" :
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
                        cantidadDetalle = v.Detalles.Count,
                        tipoDetalles = v.Detalles.Any()
                            ? string.Join(", ", v.Detalles.Select(d => d.tipo).Distinct())
                            : "Sin detalles",
                        detalles = v.Detalles.Select(d => new
                        {
                            d.idViaticoDetalle,
                            d.fecha,
                            d.tipo,
                            d.monto,
                            d.detalle
                        }).ToList()                        
                    })
                    .ToListAsync();

                return Ok(viaticos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor al aplicar filtros");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DESCARGAR VIÁTICOS FILTRADOS EN EXCEL
        public async Task<IActionResult> ExportarViaticosExcel(
            [FromQuery] string[] tiposDetalle,
            [FromQuery] string[] estados,
            [FromQuery] string[] departamentos,
            [FromQuery] string fechaTipo = null,
            [FromQuery] string fechaUnica = null,
            [FromQuery] string fechaDesde = null,
            [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.LiquidacionViatico
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                    .AsQueryable();

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
                        TiposDetalles = v.Detalles.Any()
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
                    worksheet.Cell(row, 9).Value = viatico.TiposDetalles;

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
                return File(stream,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           fileName);
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
            [FromQuery] string fechaTipo = null,
            [FromQuery] string fechaUnica = null,
            [FromQuery] string fechaDesde = null,
            [FromQuery] string fechaHasta = null)
        {
            try
            {
                var query = _context.LiquidacionViatico
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                    .AsQueryable();

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
                // Obtener todos los viáticos con sus detalles
                var viaticosConDetalles = await _context.LiquidacionViatico
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                    .Select(v => new
                    {
                        v.idViatico,
                        v.departamento,
                        v.estado,
                        v.total,
                        DetallesTipos = v.Detalles.Select(d => d.tipo).ToList(),
                        CantidadDetalles = v.Detalles.Count
                    })
                    .ToListAsync();

                // Definir los tipos de detalles que quieres contar
                var tiposInteres = new List<string>
                    {
                        "Combustible",
                        "Kilometraje",
                        "Transporte",
                        "Hospedaje",
                        "Alimentación"
                    };

                // Calcular contadores
                var contadores = new
                {
                    // Contadores por tipo de detalle
                    Combustible = viaticosConDetalles
                        .Count(v => v.DetallesTipos.Any(t => t == "Combustible")),
                    Kilometraje = viaticosConDetalles
                        .Count(v => v.DetallesTipos.Any(t => t == "Kilometraje")),
                    Transporte = viaticosConDetalles
                        .Count(v => v.DetallesTipos.Any(t => t == "Transporte")),
                    Hospedaje = viaticosConDetalles
                        .Count(v => v.DetallesTipos.Any(t => t == "Hospedaje")),
                    Alimentacion = viaticosConDetalles
                        .Count(v => v.DetallesTipos.Any(t => t == "Alimentación")),
                    OtrosTipos = viaticosConDetalles
                        .Count(v => v.DetallesTipos.Any(t =>
                            !tiposInteres.Contains(t) && !string.IsNullOrEmpty(t))),

                    // Contadores por estado
                    Creada = viaticosConDetalles
                        .Count(v => v.estado == "Creada"),
                    Aprobada = viaticosConDetalles
                        .Count(v => v.estado == "Aprobada"),
                    Pendiente = viaticosConDetalles
                        .Count(v => v.estado == "Pendiente"),
                    Rechazado = viaticosConDetalles
                        .Count(v => v.estado == "Rechazada"),

                    // Contadores por departamento
                    FinancieroContable = viaticosConDetalles
                        .Count(v => v.departamento == "Financiero Contable"),
                    Gerencia = viaticosConDetalles
                        .Count(v => v.departamento == "Gerencia"),
                    Ingenieria = viaticosConDetalles
                        .Count(v => v.departamento == "Ingeniería"),
                    Jefatura = viaticosConDetalles
                        .Count(v => v.departamento == "Jefatura"),
                    Legal = viaticosConDetalles
                        .Count(v => v.departamento == "Legal"),
                    Operaciones = viaticosConDetalles
                        .Count(v => v.departamento == "Operaciones"),
                    TecnicosNCR = viaticosConDetalles
                        .Count(v => v.departamento == "Tecnicos NCR"),
                    TecnologiasInformacion = viaticosConDetalles
                        .Count(v => v.departamento == "Tecnologías de Información"),
                    Ventas = viaticosConDetalles
                        .Count(v => v.departamento == "Ventas"),

                    // Tipos únicos encontrados (para filtros dinámicos)
                    TiposUnicos = viaticosConDetalles
                        .SelectMany(v => v.DetallesTipos)
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList()
                };

                return Ok(contadores);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    // Contadores por tipo de detalle
                    Combustible = 0,
                    Kilometraje = 0,
                    Transporte = 0,
                    Hospedaje = 0,
                    Alimentacion = 0,
                    OtrosTipos = 0,

                    // Contadores por departamento
                    FinancieroContable = 0,
                    Gerencia = 0,
                    Ingenieria = 0,
                    Jefatura = 0,
                    Legal = 0,
                    Operaciones = 0,
                    TecnicosNCR = 0,
                    TecnologiasInformacion = 0,
                    Ventas = 0,

                    // Contadores por estado
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazada = 0,
                    Creada = 0
                });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CAMBIOS DE ESTADOS MASIVOS
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMasivo([FromBody] EstadoMasivoViewModel model)
        {
            foreach (var id in model.Ids)
            {
                var viatico = await _context.LiquidacionViatico.FindAsync(id);
                if (viatico != null)
                {
                    viatico.estado = model.estado;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessageVia"] = "Se cambiaron los viaticos de estados correctamente.";
            return Ok(new { redirect = Url.Action("VerViaticos") });
        }


        // --------------------------------------------------------------------------------------
        // CREATE DE LA CABECERA DE LA LIQUIDACION
        [HttpGet]
        public async Task<IActionResult> CreateViatico()
        {
            // Detectar sistema operativo y usar el ID de zona horaria adecuado
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);

            var user = await _userManager.GetUserAsync(User);

            var liquidacion = new LiquidacionViatico
            {
                fechaCreacion = hoy,
                nombreEmpleado = user.nombreCompleto ?? "Sin nombre",
                departamento = user.departamento ?? "Sin departamento",
                jefe = "pendiente" ?? "Sin Jefe",
                estado = "Creada",
                total = 0
            };

            _context.Add(liquidacion);
            await _context.SaveChangesAsync();

            // REDIRECCIONA a Edit con el ID recién creado
            return RedirectToAction("EditViatico", new { id = liquidacion.idViatico });
        }

        // --------------------------------------------------------------------------------------
        // MUESTRA LA VISTA DE EDIT CON CABECERA Y DETALLES
        [HttpGet]
        public async Task<IActionResult> EditViatico(long id)
        {
            var liquidacion = await _context.LiquidacionViatico
                                            .Include(x => x.Detalles)
                                            .FirstOrDefaultAsync(x => x.idViatico == id);

            if (liquidacion == null) return NotFound();

            return View(liquidacion);
        }

        // --------------------------------------------------------------------------------------
        // AGREGAR DETALLE A LA LIQUIDACION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarViaticoDetalle([FromForm] AgregarDetalleViaticoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var li = await _context.LiquidacionViatico
                                       .Include(l => l.Detalles)
                                       .FirstOrDefaultAsync(l => l.idViatico == model.IdViatico);
                TempData["ErrorMessageLiq"] = "Error al validar el detalle. Revise los campos.";
                return RedirectToAction(nameof(EditViatico), new { id = model.IdViatico });
            }

            // Crear el detalle desde el ViewModel
            var detalle = new LiquidacionViaticoDetalle
            {
                idViatico = model.IdViatico,
                fecha = model.Fecha,
                tipo = model.Tipo,
                monto = model.Monto,
                detalle = model.Detalle
            };

            _context.LiquidacionViaticoDetalle.Add(detalle);
            await _context.SaveChangesAsync();

            await RecalcularTotalViatico(model.IdViatico);

            return RedirectToAction(nameof(EditViatico), new { id = model.IdViatico });
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // VER DETALLES DE VIÁTICOS
        [HttpGet]
        [Route("Liquidacion/DetailsViatico/{id}")]
        public async Task<ActionResult> DetailsViatico(long id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessageVia"] = "No se proporcionó un identificador de viático válido.";
                return RedirectToAction("VerViaticos", "Liquidacion");
            }

            // Buscar el viático con sus detalles
            var viatico = await _context.LiquidacionViatico
                .Include(v => v.Detalles)  // Incluir los detalles relacionados
                .FirstOrDefaultAsync(v => v.idViatico == id);

            if (viatico == null)
            {
                TempData["ErrorMessageVia"] = "El viático que intentas ver no existe o fue eliminado.";
                return RedirectToAction("VerViaticos", "Liquidacion");
            }

            // Crear ViewModel para mostrar datos formateados
            var viewModel = new ViaticoDetailsViewModel
            {
                idViatico = viatico.idViatico,
                fechaCreacion = viatico.fechaCreacion,
                nombreEmpleado = viatico.nombreEmpleado,
                departamento = viatico.departamento,
                jefe = viatico.jefe,
                total = viatico.total,
                estado = viatico.estado,

                // Detalles formateados
                Detalles = viatico.Detalles.Select(d => new DetalleViaticoViewModel
                {
                    idViaticoDetalle = d.idViaticoDetalle,
                    fecha = d.fecha,
                    tipo = d.tipo,
                    monto = d.monto,
                    detalle = d.detalle
                }).ToList(),

                cantidadDetalles = viatico.Detalles.Count,
                tiposDetalles = viatico.Detalles.Any()
                    ? string.Join(", ", viatico.Detalles.Select(d => d.tipo).Distinct())
                    : "Sin detalles",
            };

            return View(viewModel);
        }

        // --------------------------------------------------------------------------------------
        // ELIMINAR DETALLE DENTRO DEL EDIT DE LA LIQUIDACION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDetalle(long idDetalle, long idViatico)
        {
            var detalle = await _context.LiquidacionViaticoDetalle.FindAsync(idDetalle);
            if (detalle == null)
            {
                TempData["ErrorMessageLiq"] = "Detalle no encontrado.";
                return RedirectToAction(nameof(EditViatico), new { id = idViatico });
            }

            _context.LiquidacionViaticoDetalle.Remove(detalle);
            await _context.SaveChangesAsync();

            await RecalcularTotalViatico(idViatico);

            TempData["SuccessMessageLiq"] = "Detalle eliminado correctamente.";
            return RedirectToAction(nameof(EditViatico), new { id = idViatico });
        }

        // --------------------------------------------------------------------------------------
        // Recalcula total sumando montos de los detalles
        private async Task RecalcularTotalViatico(long idViatico)
        {
            var liqui = await _context.LiquidacionViatico
                                      .Include(l => l.Detalles)
                                      .FirstOrDefaultAsync(l => l.idViatico == idViatico);

            if (liqui == null) return;

            liqui.total = liqui.Detalles.Sum(d => d.monto);
            _context.Update(liqui);
            await _context.SaveChangesAsync();
        }
    }
}
