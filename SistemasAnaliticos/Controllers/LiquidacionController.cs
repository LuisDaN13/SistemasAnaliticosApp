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
