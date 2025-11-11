using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using static SistemasAnaliticos.Models.codigoFotos;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class BeneficioController : Controller
    {
        private readonly DBContext _context;


        public BeneficioController(DBContext context)
        {
            _context = context;

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

                var nuevo = new Beneficio
                {
                    fechaPedido = ahoraCR,
                    nombrePersona = "Luis",
                    tipo = model.tipo,

                    monto = model.monto,
                    restante = 0,
                    Comentarios = model.Comentarios,

                    estado = "Creada"
                };

                _context.Beneficio.Add(nuevo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Usuario", "Index");
            }
        }

    }
}
