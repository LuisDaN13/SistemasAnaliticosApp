using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Services
{
    public class AlcanceUsuarioService : IAlcanceUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly DBContext _context;

        public AlcanceUsuarioService(UserManager<Usuario> userManager, DBContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> ObtenerAlcanceAsync(Usuario user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var alcance = await _context.AlcanceUsuario
                .Where(r => roles.Contains(r.rolId) || roles.Contains(r.rolId))
                .Select(r => r.alcance)
                .FirstOrDefaultAsync();

            return alcance ?? "Propio";
        }
    }
}