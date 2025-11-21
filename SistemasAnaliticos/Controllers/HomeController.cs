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

                return View(fotosCarousel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageHome"] = ("Error al obtener las fotos para el carousel");
                return View(new List<FotoViewModel>());
            }
        }
    }
}
