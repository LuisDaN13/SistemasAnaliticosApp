using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
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

        // CREATE DE LA CABECERA DE LA LIQUIDACION
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
                nombreEmpleado = user.nombreCompleto,
                departamento = user.departamento,
                jefe = "pendiente",
                estado = "Creada",
                total = 0
            };

            _context.Add(liquidacion);
            await _context.SaveChangesAsync();

            // REDIRECCIONA a Edit con el ID recién creado
            return RedirectToAction("Edit", new { id = liquidacion.idViatico });
        }

        public async Task<IActionResult> EditViatico(long id)
        {
            var liquidacion = await _context.LiquidacionViatico
                                            .Include(x => x.Detalles)
                                            .FirstOrDefaultAsync(x => x.idViatico == id);

            if (liquidacion == null)
                return NotFound();

            return View(liquidacion);
        }

        public async Task<IActionResult> AgregarViaticoDetalle(LiquidacionViaticoDetalle model)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();

            // Recalcular total
            await RecalcularViaticoTotal(model.idViatico);

            return RedirectToAction("Edit", new { id = model.idViatico });
        }

        private async Task RecalcularViaticoTotal(long idViatico)
        {
            var liqui = await _context.LiquidacionViatico
                                      .Include(l => l.Detalles)
                                      .FirstAsync(l => l.idViatico == idViatico);

            liqui.total = liqui.Detalles.Sum(d => d.monto);

            await _context.SaveChangesAsync();
        }
    }
}
