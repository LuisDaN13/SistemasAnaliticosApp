using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.ViewModels;

namespace SistemasAnaliticos.Controllers
{
    public class RolPermisoController : Controller
    {
        private readonly DBContext _context;
        private readonly IRolPermisoService _rolPermisoService;

        public RolPermisoController(DBContext context, IRolPermisoService rolPermisoService)
        {
            _context = context;
            _rolPermisoService = rolPermisoService;
        }

        [Authorize(Policy = "Rol.Crear")]
        [HttpGet]
        public async Task<IActionResult> Editar(string id)
        {
            try
            {
                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                    return NotFound();

                var permisosRol = await _context.RolPermisos
                    .Where(rp => rp.RolId == id)
                    .Select(rp => rp.Clave)
                    .ToListAsync();

                var vm = new RolViewModel
                {
                    Id = rol.Id,
                    nombre = rol.Name,
                    estado = true,

                    Permisos = PermisosSistema.Todos.Select(p => new PermisoChecksViewModel
                    {
                        Clave = p,
                        Seleccionado = permisosRol.Contains(p)
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageRoles"] = "Ocurrió un error al cargar los permisos: " + ex.Message;
                return RedirectToAction("Index", "Rol");
            }
        }

        [Authorize(Policy = "Rol.Crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(RolViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Id))
                {
                    TempData["ErrorMessageRoles"] = "Rol inválido.";
                    return RedirectToAction("Index", "Rol");
                }

                // Extraer SOLO los permisos seleccionados
                var permisosSeleccionados = model.Permisos
                    .Where(p => p.Seleccionado)
                    .Select(p => p.Clave)
                    .ToList();

                await _rolPermisoService.AsignarPermisosARolAsync(
                    model.Id,
                    permisosSeleccionados
                );

                TempData["SuccessMessageRoles"] = "Permisos actualizados correctamente.";
                return RedirectToAction("Index", "Rol");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageRoles"] = "Error al guardar permisos: " + ex.Message;
                return RedirectToAction("Index", "Rol");
            }
        }

    }
}
