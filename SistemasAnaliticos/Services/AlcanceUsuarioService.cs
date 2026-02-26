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

            if (roles.Contains("Administrador") || roles.Contains("RRHH"))
                return "Global";

            if (roles.Contains("Jefatura"))
                return "Subordinados";

            return "Propio";
        }

        public async Task<string> ObtenerAlcanceFinancieroAsync(Usuario user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Administrador") || roles.Contains("Financiero"))
                return "Global";

            return "Propio";
        }
    }
}