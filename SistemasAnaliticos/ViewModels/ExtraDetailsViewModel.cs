namespace SistemasAnaliticos.ViewModels
{
    public class ExtraDetailsViewModel
    {
        public long idExtra { get; set; }
        public DateOnly fechaCreacion { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string jefe { get; set; }
        public decimal totalHoras { get; set; }
        public string estado { get; set; }
        public int cantidadDetalles { get; set; }
        public string lugares { get; set; }

        // Detalles
        public List<DetalleExtraViewModel> Detalles { get; set; } = new();
    }
}
