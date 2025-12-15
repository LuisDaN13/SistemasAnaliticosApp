using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Auxiliares
{
    public class PermisoHandler : AuthorizationHandler<PermisoRequirement>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly DBContext _context;

        public PermisoHandler(UserManager<Usuario> userManager, DBContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext contextAuth,
            PermisoRequirement requirement)
        {
            var user = contextAuth.User;
            if (!user.Identity.IsAuthenticated)
                return;

            var usuario = await _userManager.GetUserAsync(user);
            if (usuario == null)
                return;

            var roles = await _userManager.GetRolesAsync(usuario);

            var tienePermiso = _context.RolPermisos.Any(rp =>
                roles.Contains(
                    _context.Roles
                        .Where(r => r.Id == rp.RolId)
                        .Select(r => r.Name)
                        .FirstOrDefault()
                )
                && rp.Clave == requirement.ClavePermiso
            );

            if (tienePermiso)
            {
                contextAuth.Succeed(requirement);
            }
        }
    }
}