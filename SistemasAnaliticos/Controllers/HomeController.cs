using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.ViewModels;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBContext _context;

        public HomeController(DBContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX
        public async Task<ActionResult> Index()
        {
            try
            {
                var fotosCarousel = await _context.Fotos
                    .Where(f => f.estado == true)
                    .Select(f => new FotoViewModel
                    {
                        foto = f.foto
                    })
                    .ToListAsync();

                var noticias = await _context.Noticias
                    .Where(t => t.estado == true)
                    .Take(5)
                    .OrderByDescending(t => t.fechaPublicacion)
                    .ThenByDescending(t => t.horaPublicacion)
                    .Select(t => new NoticiaViewModel
                    {
                        idNoticia = t.idNoticia,
                        titulo = t.titulo,
                        categoria = t.categoria,
                        foto = t.foto,
                        autor = t.autor,
                        fechaPublicacion = t.fechaPublicacion,
                        horaPublicacion = t.horaPublicacion,
                    })
                    .ToListAsync();

                var vm = new HomeIndexViewModel
                {
                    FotosCarousel = fotosCarousel,
                    Noticias = noticias
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageHome"] = "Error al obtener los datos.";

                return View(new HomeIndexViewModel
                {
                    FotosCarousel = new List<FotoViewModel>(),
                    Noticias = new List<NoticiaViewModel>()
                });
            }
        }

        public ActionResult HomeInicial()
        {
            return View();
        }

        public ActionResult HomeFinanciero()
        {
            return View();
        }
    }
}
