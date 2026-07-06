using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Services
{
    public interface IAlcanceUsuarioService
    {
        Task<string> ObtenerAlcanceAsync(Usuario user);
        Task<string> ObtenerAlcanceFinancieroAsync(Usuario user);
        Task<string> ObtenerAlcanceGarantiaAsync(Usuario user);
    }
}
