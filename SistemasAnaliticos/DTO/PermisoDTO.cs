using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.DTO
{
    public class PermisoDTO
    {
        public long idPermiso { get; set; }
        public string nombreEmpleado { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
        public DateTime fechaIngreso { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFinalizacion { get; set; }
        public DateTime? fechaRegresoLaboral { get; set; }
        public string motivo { get; set; }
        public string comentarios { get; set; }
        public byte[] foto { get; set; }
        public string nombreArchivo { get; set; }
        public string tipoMIME { get; set; }
        public long? tamanoArchivo { get; set; }
    }
}
