namespace SistemasAnaliticos.ViewModels
{
    public class RolViewModel
    {
        public string Id { get; set; }
        public string nombre { get; set; }
        public bool estado { get; set; }


        public List<PermisoChecksViewModel> Permisos { get; set; }
    }
}
