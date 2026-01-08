namespace SistemasAnaliticos.ViewModels
{
    public class ExtraViewModel
    {
        public long idExtra { get; set; }
        public DateOnly? fechaCreacion { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string jefe { get; set; }
        public decimal total { get; set; }
        public decimal cantidadDetalle { get; set; }
        public string lugares { get; set; }

        public string estado { get; set; }
        public string tipoExtra { get; set; }
    }
}
