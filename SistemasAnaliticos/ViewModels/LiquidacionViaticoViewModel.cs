namespace SistemasAnaliticos.ViewModels
{
    public class LiquidacionViaticoViewModel
    {
        public long idViatico { get; set; }
        public DateOnly? fechaCreacion { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string jefe { get; set; }
        public decimal total { get; set; }
        public decimal cantidadDetalle { get; set; }
        public string tipoDetalles { get; set; }
        public string estado { get; set; }
    }
}
