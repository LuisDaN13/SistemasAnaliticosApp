using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;

namespace SistemasAnaliticos.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> userManager;

        public AuditoriaController(DBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var auditorias = await _context.Auditoria
                .AsNoTracking()
                .OrderByDescending(x => x.idAudit)
                .Select(x => new AuditViewModel
                {
                    Fecha = x.Fecha,
                    Hora = x.Hora,
                    Usuario = x.Usuario,
                    Tabla = x.Tabla,
                    Accion = x.Accion,
                })
                .ToListAsync();

            return View(auditorias);
        }
    }
}
