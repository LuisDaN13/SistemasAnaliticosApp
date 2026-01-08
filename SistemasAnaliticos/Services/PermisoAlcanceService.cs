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


        // PARA EXTRAS
        public async Task<IQueryable<Extras>> AplicarAlcanceExtrasConTipoAsync(
            IQueryable<Extras> query,
            ClaimsPrincipal user)
        {
            var usuario = await _userManager.GetUserAsync(user);
            var alcance = await _alcanceUsuarioService.ObtenerAlcanceAsync(usuario);
            var departamentoUsuario = usuario.departamento; // propiedad de departamento

            // Administrador y RRHH ven todo (Global)
            if (alcance == "Global")
                return query;

            // Jefatura ve subordinados + propios
            if (alcance == "Subordinados")
                return await FiltrarSubordinadosExtrasAsync(query, usuario);

            // Para usuarios con alcance "Propio", aplicar la lógica especial
            var queryPropio = query.Where(p => p.UsuarioId == usuario.Id);

            // AGREGAR LA LÓGICA ESPECIAL: si el usuario pertenece a FinancieroContable u Operaciones
            // y el Extra es de tipo "Técnico", también debe verlo
            var departamentosEspeciales = new List<string> { "FinancieroContable", "Operaciones" };

            if (departamentosEspeciales.Contains(departamentoUsuario))
            {
                // Unir: sus propios Extras + los Extras de tipo "Técnico"
                var extrasTecnicos = query.Where(e => e.tipoExtra == "Técnico");
                queryPropio = queryPropio.Union(extrasTecnicos).Distinct();
            }

            return queryPropio;
        }

        private async Task<IQueryable<Extras>> FiltrarSubordinadosExtrasAsync(
            IQueryable<Extras> query,
            Usuario usuario)
        {
            var subordinadosIds = await _context.Users
                .Where(u => u.jefeId == usuario.Id)
                .Select(u => u.Id)
                .ToListAsync();

            // Añadir al usuario actual
            subordinadosIds.Add(usuario.Id);

            // Primero: filtrar por subordinados
            var querySubordinados = query.Where(p => subordinadosIds.Contains(p.UsuarioId));

            // Si el jefe pertenece a FinancieroContable u Operaciones, agregar Extras Técnicos
            var departamentosEspeciales = new List<string> { "FinancieroContable", "Operaciones" };
            var departamentoJefe = usuario.departamento;

            if (departamentosEspeciales.Contains(departamentoJefe))
            {
                var extrasTecnicos = query.Where(e => e.tipoExtra == "Técnico");
                querySubordinados = querySubordinados.Union(extrasTecnicos).Distinct();
            }

            return querySubordinados;
        }
    }
}