using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.DTO
{
    public class PaginacionPermisosDTO
    {
        public List<PermisoDTO> Permisos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

}
