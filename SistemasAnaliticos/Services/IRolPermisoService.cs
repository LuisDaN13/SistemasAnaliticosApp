namespace SistemasAnaliticos.Services
{
    public interface IRolPermisoService
    {
        Task AsignarPermisosARolAsync(string rolId, List<string> permisos);
    }
}
