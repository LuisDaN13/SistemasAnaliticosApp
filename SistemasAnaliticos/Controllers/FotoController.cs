using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Controllers
{
    public class FotoController : Controller
    {
        private readonly DBContext _context;

        public FotoController(DBContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var fotos = await _context.Fotos
                .AsNoTracking()
                .Select(x => new FotoDTO
                {
                    idFoto = x.idFoto,
                    foto = x.foto,
                    estado = x.estado
                })
                .ToListAsync();

            return View(fotos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    TempData["ErrorMessageFotos"] = "Por favor seleccione una foto";
                    return RedirectToAction("Index");
                }

                // Validar que sea una imagen
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
                var fileExtension = Path.GetExtension(foto.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessageFotos"] = "Formato de archivo no válido. Solo se permiten imágenes (JPG, JPEG, PNG)";
                    return RedirectToAction("Index");
                }

                // Validar tamaño del archivo (ejemplo: máximo 5MB)
                if (foto.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessageFotos"] = "La imagen es demasiado grande. Tamaño máximo permitido: 5MB";
                    return RedirectToAction("Index");
                }

                // Convertir la imagen a byte[]
                using var memoryStream = new MemoryStream();
                await foto.CopyToAsync(memoryStream);
                var fotoBytes = memoryStream.ToArray();

                // Crear el DTO
                var nuevo = new Fotos
                {
                    foto = fotoBytes,
                    estado =  true
                };

                // Llamar a tu servicio/repositorio
                _context.Fotos.Add(nuevo);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageFotos"] = "Foto subida exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log del error
                TempData["ErrorMessageFotos"] = "Error al subir la foto. Por favor intente nuevamente.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost("Foto/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                var foto = await _context.Fotos.FindAsync(id);
                if (foto == null)
                {
                    TempData["ErrorMessageFotos"] = "Foto no encontrada";
                    return RedirectToAction("Index");
                }

                _context.Fotos.Remove(foto);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageFotos"] = "Se eliminó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageFotos"] = "Ocurrió un error al agregar la foto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost("Foto/Inactivar/{id}")]
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

                TempData["SuccessMessageFotos"] ="Se cambió correctamente.";
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