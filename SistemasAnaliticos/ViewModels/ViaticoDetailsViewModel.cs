namespace SistemasAnaliticos.ViewModels
{
    public class ViaticoDetailsViewModel
    {
        public long idViatico { get; set; }
        public DateOnly fechaCreacion { get; set; }
        public string fechaCreacionFormateada { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string jefe { get; set; }
        public decimal total { get; set; }
        public string estado { get; set; }
        public int cantidadDetalles { get; set; }
        public string tiposDetalles { get; set; }

        // Detalles
        public List<DetalleViaticoViewModel> Detalles { get; set; } = new();
    }
}
