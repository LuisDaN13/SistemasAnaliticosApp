namespace SistemasAnaliticos.ViewModels
{
    public class BeneficioViewModel
    {
        public long idBeneficio { get; set; }
        public DateOnly? fechaCreacion { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string tipo { get; set; }
        public decimal monto { get; set; }
        public DateTime? fechaRequerida { get; set; }
        public string comentarios { get; set; }
        public string estado { get; set; }
    }
}
