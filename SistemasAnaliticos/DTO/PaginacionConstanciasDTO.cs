using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.DTO
{
    public class PaginacionConstanciasDTO
    {
        public List<ConstanciaDTO> Constancias { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

}
