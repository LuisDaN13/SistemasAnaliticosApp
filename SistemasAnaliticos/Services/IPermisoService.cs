using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Services
{
    public interface IPermisoService
    {
        Task<List<Permiso>> TraerCitas();
        Task<List<Permiso>> TraerVacaciones();
        Task<List<Permiso>> TraerIncapacidades();
        Task<List<Permiso>> TraerTeletrabajo();
        Task<List<Permiso>> TraerPerEspecial();
        Task<List<Constancia>> TraerConstanciaLaboral();
        Task<List<Constancia>> TraerConstanciaSalarial();
        Task<List<Beneficio>> TraerBenePrestamos();
        Task<List<Beneficio>> TraerBeneAdelanto();
    }
}
