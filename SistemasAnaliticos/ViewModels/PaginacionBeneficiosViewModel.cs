namespace SistemasAnaliticos.ViewModels
{
    public class PaginacionBeneficiosViewModel
    {
        public List<BeneficioViewModel> Beneficios { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
