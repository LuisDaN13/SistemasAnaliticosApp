using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.ViewModels;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class RolPermisoController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IRolPermisoService _rolPermisoService;

        public RolPermisoController(DBContext context, UserManager<Usuario> userManager, IRolPermisoService rolPermisoService)
        {
            _context = context;
            _userManager = userManager;
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

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await _userManager.GetUserAsync(User);
                var rol = await _context.Roles.FindAsync(model.Id);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Permisos de Rol",
                    Accion = "Cambios de Permisos en el rol de" + rol.Name,
                };
                _context.Auditoria.Add(auditoria);

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
