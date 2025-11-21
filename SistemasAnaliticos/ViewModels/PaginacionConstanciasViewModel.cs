namespace SistemasAnaliticos.ViewModels
{
    public class PaginacionConstanciasViewModel
    {
        public List<ConstanciaViewModel> Constancias { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

}
