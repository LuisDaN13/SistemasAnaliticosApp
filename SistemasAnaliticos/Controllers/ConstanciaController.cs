using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Models.codigoFotos;
using SistemasAnaliticos.Services;

namespace SistemasAnaliticos.Controllers
{
    public class ConstanciaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IConstanciaService _constanciaService;


        public ConstanciaController(DBContext context, UserManager<Usuario> userManager, IConstanciaService constanciaService)
        {
            _context = context;
            _userManager = userManager;
            _constanciaService = constanciaService;
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

                var user = await _userManager.GetUserAsync(User);

                var pdfBytes = await _constanciaService.GenerarConstanciaLaboral(user.nombreCompleto, user.cedula, user.departamento, user.puesto);

                var nuevo = new Constancia
                {
                    fechaPedido = ahoraCR,
                    nombrePersona = "Luis",
                    tipo = model.tipo,

                    dirijido = model.dirijido,
                    fechaRequerida = model.fechaRequerida,
                    Comentarios = model.Comentarios,

                    datosAdjuntos = pdfBytes,
                    nombreArchivo = $"Constancia_Laboral_{user.nombreCompleto}.pdf",
                    tipoMIME = "application/pdf",
                    tamanoArchivo = pdfBytes.Length,

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



        public async Task<IActionResult> ProbarPDF()
        {
            try
            {
                var constanciaService = new ConstanciaService();

                // Datos de prueba
                var pdfBytes = await constanciaService.GenerarConstanciaLaboral(
                    "Juan Pérez García",
                    "1-2345-6789",
                    "Recursos Humanos",
                    "Analista Senior"
                );

                // Retornar el PDF directamente para visualizarlo
                return File(pdfBytes, "application/pdf", "Constancia_Prueba.pdf");
            }
            catch (Exception ex)
            {
                return Content($"Error al generar PDF: {ex.Message}");
            }
        }
    }
}
