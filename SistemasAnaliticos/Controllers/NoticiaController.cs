using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Controllers
{
    public class NoticiaController : Controller
    {
        private readonly DBContext _context;

        public NoticiaController(DBContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var fotos = await _context.Noticias
                .AsNoTracking()
                .Select(x => new NoticiaDTO
                {
                    idNoticia = x.idNoticia,
                    titulo = x.titulo,
                    categoria = x.categoria,
                    foto = x.foto,
                    fechaPublicacion = x.fechaPublicacion,
                    horaPublicacion = x.horaPublicacion,
                    autor = x.autor,
                    contenidoTexto = x.contenidoTexto
                })
                .ToListAsync();

            return View(fotos);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        // POST: NoticiaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: NoticiaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: NoticiaController/Edit/5
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

        // GET: NoticiaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: NoticiaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
    }
}
