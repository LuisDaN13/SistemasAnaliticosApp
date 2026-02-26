using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Services
{
    public interface IAlcanceUsuarioService
    {
        Task<string> ObtenerAlcanceAsync(Usuario user);
        Task<string> ObtenerAlcanceFinancieroAsync(Usuario user);
    }
}
