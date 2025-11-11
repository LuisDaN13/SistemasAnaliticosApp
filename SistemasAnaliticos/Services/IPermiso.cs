using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Services
{
    public interface IPermiso
    {
        Task<List<Permiso>> TraerIncapacidades();
        Task<List<Permiso>> TraerCitas();
        Task<List<Permiso>> TraerVacaciones();
        Task<List<Permiso>> TraerTeletrabajo();
        Task<List<Permiso>> TraerEspecial();
        Task<List<Constancia>> TraerConstanciaLaboral();
        Task<List<Constancia>> TraerConstanciaSalarial();
        Task<List<Beneficio>> TraerBeneficio();
        Task<List<Beneficio>> TraerBeneficio();
    }
}
