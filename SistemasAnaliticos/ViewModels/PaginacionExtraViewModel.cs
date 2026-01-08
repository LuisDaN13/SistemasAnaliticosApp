namespace SistemasAnaliticos.ViewModels
{
    public class PaginacionLiquidacionViaticoViewModel
    {
        public List<LiquidacionViaticoViewModel> LiquidacionesViaticos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
