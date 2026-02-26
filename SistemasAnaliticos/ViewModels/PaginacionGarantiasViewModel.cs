namespace SistemasAnaliticos.ViewModels
{
    public class PaginacionGarantiasViewModel
    {
        public List<GarantiaRegistroViewModel> Garantias { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

}
