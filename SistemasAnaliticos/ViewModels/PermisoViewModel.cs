namespace SistemasAnaliticos.ViewModels
{
    public class PermisoViewModel
    {
        public long idPermiso { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
        public DateOnly fechaCreacion { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFinalizacion { get; set; }
        public DateTime? fechaRegresoLaboral { get; set; }
        public TimeSpan? horaCita { get; set; }
        public string motivo { get; set; }
        public string comentarios { get; set; }
        public byte[] foto { get; set; }
        public string nombreArchivo { get; set; }
        public string tipoMIME { get; set; }
        public long? tamanoArchivo { get; set; }
    }
}
