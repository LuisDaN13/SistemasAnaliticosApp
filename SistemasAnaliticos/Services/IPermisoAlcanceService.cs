using SistemasAnaliticos.Entidades;
using System.Security.Claims;

namespace SistemasAnaliticos.Services
{
    public interface IPermisoAlcanceService
    {
        Task<IQueryable<Permiso>> AplicarAlcancePermisoAsync(IQueryable<Permiso> query, ClaimsPrincipal user);
        Task<IQueryable<Constancia>> AplicarAlcanceConstanciaAsync(IQueryable<Constancia> query, ClaimsPrincipal user);
        Task<IQueryable<Beneficio>> AplicarAlcanceBeneficioAsync(IQueryable<Beneficio> query, ClaimsPrincipal user);
        Task<IQueryable<LiquidacionViatico>> AplicarAlcanceViaticoAsync(IQueryable<LiquidacionViatico> query, ClaimsPrincipal user);
    }
}
