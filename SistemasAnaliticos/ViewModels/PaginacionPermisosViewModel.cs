namespace SistemasAnaliticos.ViewModels
{
    public class PaginacionPermisosViewModel
    {
        public List<PermisoViewModel> Permisos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

}
