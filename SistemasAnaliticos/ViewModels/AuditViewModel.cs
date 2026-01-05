namespace SistemasAnaliticos.ViewModels
{
    public class AuditViewModel
    {
        public DateOnly Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Usuario { get; set; }
        public string Tabla { get; set; }
        public string Accion { get; set; }
    }
}
