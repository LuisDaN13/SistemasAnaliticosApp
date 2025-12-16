using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
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
        [Authorize(Policy = "Noticia.Ver")]
        public async Task<ActionResult> Index()
        {
            var fotos = await _context.Noticias
                .AsNoTracking()
                .OrderByDescending(x => x.fechaPublicacion)
                .ThenByDescending(x => x.horaPublicacion)
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
        [Authorize(Policy = "Noticia.Detalles")]
        public async Task<ActionResult> Details(long id)
        {
            if (id == null)
            {
                TempData["ErrorMessageHome"] = "No se proporcionó un identificador de noticia válido.";
                return RedirectToAction("Index", "Home");
            }

            var noticia = await _context.Noticias.FindAsync(id);

            if (noticia == null)
            {
                TempData["ErrorMessageHome"] = "La noticia que intentas editar no existe o fue eliminado.";
                return RedirectToAction("Index", "Home");
            }

            return View(noticia);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CREAR REGISTRO DE NOTICIAS
        [Authorize(Policy = "Noticia.Crear")]
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
                    autor = $"{user.primerNombre} {user.primerApellido}",
                    departamento = user.departamento,
                    contenidoTexto = model.contenidoTexto,
                    foto = fotoService.ConvertFileToByteArrayAsync(model.fotoFile).Result,
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
        // INACTIVAR NOTICIAS
        [Authorize(Policy = "Noticia.Inactivar")]
        [HttpPost("Noticia/Inactivar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inactivar(long id)
        {
            try
            {
                var noticia = await _context.Noticias.FindAsync(id);
                if (noticia == null)
                {
                    TempData["ErrorMessageNoticias"] = "Noticia no encontrada";
                    return RedirectToAction("Index");
                }

                noticia.estado = !noticia.estado;
                await _context.SaveChangesAsync();

                TempData["SuccessMessageNoticias"] = "Se cambió correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageNoticias"] = "Ocurrió un error al inactivar la noticia: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // ELIMINAR NOTICIAS
        [Authorize(Policy = "Noticia.Eliminar")]
        [HttpPost("Noticia/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                var noticia = await _context.Noticias.FindAsync(id);
                if (noticia == null)
                {
                    TempData["ErrorMessageNoticias"] = "Noticia no encontrada.";
                    return RedirectToAction("Index");
                }

                _context.Noticias.Remove(noticia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageNoticias"] = "Se eliminó correctamente la noticia.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageNoticias"] = "Ocurrió un error al eliminar la noticia: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
