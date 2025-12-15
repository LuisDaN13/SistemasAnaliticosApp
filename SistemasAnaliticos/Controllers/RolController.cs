using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;

namespace SistemasAnaliticos.Controllers
{
    public class RolController : Controller
    {
        private readonly DBContext _context;

        public RolController(DBContext context)
        {
            _context = context;
        }

        // GET: RolController
        public async Task<ActionResult> Index()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .Select(x => new RolViewModel
                {
                    Id = x.Id,
                    nombre = x.Name,
                    estado = x.estado
                })
                .ToListAsync();

            return View(roles);
        }

        // POST: RolController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RolViewModel model)
        {
            try
            {
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(model.nombre))
                {
                    TempData["ErrorMessageRoles"] = "El nombre del rol es requerido";
                    return RedirectToAction("Index");
                }

                // Crear el rol
                var nuevoRol = new Rol
                {
                    Name = model.nombre.Trim(),
                    estado = true
                };

                // Verificar si ya existe un rol con el mismo nombre
                var rolExistente = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == nuevoRol.Name);

                if (rolExistente != null)
                {
                    TempData["ErrorMessageRoles"] = "Ya existe un rol con ese nombre";
                    return RedirectToAction("Index");
                }

                // Llamar a tu servicio/repositorio
                _context.Roles.Add(nuevoRol);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageRoles"] = "Rol creado exitosamente";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                // Log del error específico de base de datos
                TempData["ErrorMessageRoles"] = "Error en la base de datos al crear el rol";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log del error general
                TempData["ErrorMessageRoles"] = "Error al crear el rol. Por favor intente nuevamente.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Rol/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                {
                    TempData["ErrorMessageRoles"] = "Rol no encontrada";
                    return RedirectToAction("Index");
                }

                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageRoles"] = "Se eliminó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageRoles"] = "Ocurrió un error al eliminar el rol: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Rol/Inactivar")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inactivar(string id)
        {
            try
            {
                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                {
                    TempData["ErrorMessageRoles"] = "Rol no encontrada";
                    return RedirectToAction("Index");
                }

                rol.estado = !rol.estado;
                await _context.SaveChangesAsync();

                TempData["SuccessMessageRoles"] = "Se cambió correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageRoles"] = "Ocurrió un error al inactivar el rol: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
