using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
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

        public IActionResult Index()
        {
            return View();
        }

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
                    Comentarios = model.Comentarios,

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
