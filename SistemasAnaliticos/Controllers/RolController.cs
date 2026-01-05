using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class RolController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> userManager;

        public RolController(DBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX
        [Authorize(Policy = "Rol.Ver")]
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


        [HttpGet]
        public async Task<IActionResult> MostrarUsers()
        {
            var usuarios = userManager.Users.ToList();
            var resultado = new List<object>();

            foreach (var user in usuarios)
            {
                var rol = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                resultado.Add(new
                {
                    user.Id,
                    user.nombreCompleto,
                    user.UserName,
                    Rol = rol ?? "Sin rol asignado"
                });
            }

            ViewBag.Usuarios = resultado;

            // Si es una solicitud AJAX, devolver la vista parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UsuariosList");
            }

            return View();
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // Crear
        [Authorize(Policy = "Rol.Crear")]
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
                    NormalizedName = model.nombre.ToUpper(),
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

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Rol",
                    Accion = "Nuevo registro del rol " + nuevoRol.Name,
                };
                _context.Auditoria.Add(auditoria);

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

        // -------------------------------------------------------------------------------------------------------------------------------
        // Eliminar Rol
        [Authorize(Policy = "Rol.Eliminar")]
        [HttpPost("Rol/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var rol = await _context.Roles.FindAsync(id);
                var rolNombre = rol?.Name;
                if (rol == null)
                {
                    TempData["ErrorMessageRoles"] = "Rol no encontrada";
                    return RedirectToAction("Index");
                }

                _context.Roles.Remove(rol);

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Rol",
                    Accion = "Eliminación del rol " + rolNombre,
                };
                _context.Auditoria.Add(auditoria);

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

        // -------------------------------------------------------------------------------------------------------------------------------
        // Inactivar Rol
        [Authorize(Policy = "Rol.Inactivar")]
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

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Rol",
                    Accion = "Inactivación del rol " + rol.Name,
                };
                _context.Auditoria.Add(auditoria);

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
