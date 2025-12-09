using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;
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
                .OrderByDescending(x => x.fechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ConstanciaViewModel
                {
                    //idConstancia = p.idConstancia,
                    //fechaCreacion = p.fechaCreacion,
                    //nombreEmpleado = p.nombreEmpleado,
                    //departamento = p.departamento,
                    //tipo = p.tipo,
                    //dirijido = p.dirijido,
                    //fechaRequerida = p.fechaRequerida,
                    //comentarios = p.comentarios,
                    //nombreArchivo = p.nombreArchivo,
                    //tipoMIME = p.tipoMIME,
                    //tamanoArchivo = p.tamanoArchivo,
                    //estado = p.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionConstanciasViewModel
            {
                //Constancias = viaticos,
                //PaginaActual = page,
                //TotalPaginas = (int)Math.Ceiling(totalViaticos / (double)pageSize)
            };

            return View(viewModel);
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
