using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.ViewModels;

namespace SistemasAnaliticos.Controllers
{
    public class OrganigramaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;

        public OrganigramaController(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var query = _userManager.Users
                .AsNoTracking()
                .Where(x => x.primerApellido != "Admin" && x.primerNombre != "Montilla");

            var usuarios = await query
                .Where(u => u.estado == true)
                .Select(u => new OrganigramaVM 
                {
                    Id = u.Id,
                    Nombre = (u.primerNombre + " " + u.primerApellido).Trim(),
                    Puesto = u.puesto ?? "",
                    Departamento = u.departamento ?? "",
                    CorreoEmpresa = u.correoEmpresa ?? "",
                    JefeId = u.jefeId
                })
                .ToListAsync();

            return View(usuarios);
        }
    }
}
