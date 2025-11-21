using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.ViewModels;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Models.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class NoticiaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;

        public NoticiaController(DBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX
        public async Task<ActionResult> Index()
        {
            var fotos = await _context.Noticias
                .AsNoTracking()
                .Select(x => new NoticiasCardsViewModel
                {
                    Id = x.idNoticia,
                    Titulo = x.titulo,
                    Autor = x.autor,
                    Foto = x.foto,
                    Estado = x.estado
                })
                .ToListAsync();

            return View(fotos);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // VER DETALLES DE NOTICIAS
        public ActionResult Details(int id)
        {
            return View();
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CREAR REGISTRO DE NOTICIAS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Noticias model)
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
                var user = await _userManager.GetUserAsync(User);

                var nuevo = new Noticias
                {
                    titulo = model.titulo,
                    categoria = model.categoria,
                    fechaPublicacion = hoy,
                    horaPublicacion = ahoraCR.TimeOfDay,
                    //autor = $"{user.primerNombre} {user.primerApellido}",
                    autor = "Luis",
                    contenidoTexto = model.contenidoTexto,
                    foto = await fotoService.ProcesarArchivo(model.fotoFile, null),
                    estado = true
                };
                _context.Noticias.Add(nuevo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessageNoticias"] = "Se creó la noticia correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessageNoticias"] = "Error en la creación de la noticias.";
                return RedirectToAction(nameof(Index));
            }
        }


        // -------------------------------------------------------------------------------------------------------------------------------
        // EDITAR UNA NOTICIA
        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INACTIVAR NOTICIAS
        [HttpPost("Noticia/Inactivar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inactivar(long id)
        {
            try
            {
                var foto = await _context.Fotos.FindAsync(id);
                if (foto == null)
                {
                    TempData["ErrorMessageFotos"] = "Foto no encontrada";
                    return RedirectToAction("Index");
                }

                foto.estado = !foto.estado;
                await _context.SaveChangesAsync();

                TempData["SuccessMessageFotos"] = "Se cambió correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageFotos"] = "Ocurrió un error al agregar la foto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
