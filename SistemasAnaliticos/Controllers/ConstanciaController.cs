using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using static SistemasAnaliticos.Models.codigoFotos;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class ConstanciaController : Controller
    {
        private readonly DBContext _context;


        public ConstanciaController(DBContext context)
        {
            _context = context;

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Create(Constancia model)
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

                var nuevo = new Constancia
                {
                    fechaPedido = ahoraCR,
                    nombrePersona = "Luis",
                    tipo = model.tipo,

                    dirijido = model.dirijido,
                    fechaRequerida = model.fechaRequerida,
                    Comentarios = model.Comentarios,

                    estado = "Creada"
                };

                _context.Constancia.Add(nuevo);
                await _context.SaveChangesAsync();
                return RedirectToAction("Permiso", "Index");
            }
            catch
            {
                return RedirectToAction("Usuario", "Index");
            }
        }

    }
}
