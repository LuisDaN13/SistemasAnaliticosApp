namespace SistemasAnaliticos.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using SistemasAnaliticos.Entidades;
    using SistemasAnaliticos.Models;
    using System.Security.Claims;

    public class PermisoAlcanceService : IPermisoAlcanceService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly DBContext _context;
        private readonly IAlcanceUsuarioService _alcanceUsuarioService;

        public PermisoAlcanceService(
            UserManager<Usuario> userManager,
            DBContext context,
            IAlcanceUsuarioService alcanceUsuarioService)
        {
            _userManager = userManager;
            _context = context;
            _alcanceUsuarioService = alcanceUsuarioService;
        }

        // PARA PERMISOS   
        public async Task<IQueryable<Permiso>> AplicarAlcancePermisoAsync(
            IQueryable<Permiso> query,
            ClaimsPrincipal user)
        {
            var usuario = await _userManager.GetUserAsync(user);
            var alcance = await _alcanceUsuarioService.ObtenerAlcanceAsync(usuario);

            return alcance switch
            {
                "Global" => query,

                "Subordinados" => await FiltrarSubordinadosPermisoAsync(query, usuario),

                _ => query.Where(p => p.UsuarioId == usuario.Id)
            };
        }

        private async Task<IQueryable<Permiso>> FiltrarSubordinadosPermisoAsync(
            IQueryable<Permiso> query,
            Usuario usuario)
        {
            var subordinadosIds = await _context.Users
                .Where(u => u.jefeId == usuario.Id)
                .Select(u => u.Id)
                .ToListAsync();

            // AÑADIR AL USUARIO ACTUAL A LA LISTA
            subordinadosIds.Add(usuario.Id);

            return query.Where(p => subordinadosIds.Contains(p.UsuarioId));
        }


        // PARA CONSTANCIAS   
        public async Task<IQueryable<Constancia>> AplicarAlcanceConstanciaAsync(
            IQueryable<Constancia> query,
            ClaimsPrincipal user)
        {
            var usuario = await _userManager.GetUserAsync(user);
            var alcance = await _alcanceUsuarioService.ObtenerAlcanceAsync(usuario);

            return alcance switch
            {
                "Global" => query,

                "Subordinados" => await FiltrarSubordinadosConstanciaAsync(query, usuario),

                _ => query.Where(p => p.UsuarioId == usuario.Id)
            };
        }

        private async Task<IQueryable<Constancia>> FiltrarSubordinadosConstanciaAsync(
            IQueryable<Constancia> query,
            Usuario usuario)
        {
            var subordinadosIds = await _context.Users
                .Where(u => u.jefeId == usuario.Id)
                .Select(u => u.Id)
                .ToListAsync();

            // AÑADIR AL USUARIO ACTUAL A LA LISTA
            subordinadosIds.Add(usuario.Id);

            return query.Where(p => subordinadosIds.Contains(p.UsuarioId));
        }


        // PARA BENEFICIOS   
        public async Task<IQueryable<Beneficio>> AplicarAlcanceBeneficioAsync(
            IQueryable<Beneficio> query,
            ClaimsPrincipal user)
        {
            var usuario = await _userManager.GetUserAsync(user);
            var alcance = await _alcanceUsuarioService.ObtenerAlcanceAsync(usuario);

            return alcance switch
            {
                "Global" => query,

                "Subordinados" => await FiltrarSubordinadosBeneficioAsync(query, usuario),

                _ => query.Where(p => p.UsuarioId == usuario.Id)
            };
        }

        private async Task<IQueryable<Beneficio>> FiltrarSubordinadosBeneficioAsync(
            IQueryable<Beneficio> query,
            Usuario usuario)
        {
            var subordinadosIds = await _context.Users
                .Where(u => u.jefeId == usuario.Id)
                .Select(u => u.Id)
                .ToListAsync();

            // AÑADIR AL USUARIO ACTUAL A LA LISTA
            subordinadosIds.Add(usuario.Id);

            return query.Where(p => subordinadosIds.Contains(p.UsuarioId));
        }


        // PARA VIATICOS   
        public async Task<IQueryable<LiquidacionViatico>> AplicarAlcanceViaticoAsync(
            IQueryable<LiquidacionViatico> query,
            ClaimsPrincipal user)
        {
            var usuario = await _userManager.GetUserAsync(user);
            var alcance = await _alcanceUsuarioService.ObtenerAlcanceAsync(usuario);

            return alcance switch
            {
                "Global" => query,

                "Subordinados" => await FiltrarSubordinadosViaticoAsync(query, usuario),

                _ => query.Where(p => p.UsuarioId == usuario.Id)
            };
        }

        private async Task<IQueryable<LiquidacionViatico>> FiltrarSubordinadosViaticoAsync(
            IQueryable<LiquidacionViatico> query,
            Usuario usuario)
        {
            var subordinadosIds = await _context.Users
                .Where(u => u.jefeId == usuario.Id)
                .Select(u => u.Id)
                .ToListAsync();

            // AÑADIR AL USUARIO ACTUAL A LA LISTA
            subordinadosIds.Add(usuario.Id);

            return query.Where(p => subordinadosIds.Contains(p.UsuarioId));
        }
    }
}