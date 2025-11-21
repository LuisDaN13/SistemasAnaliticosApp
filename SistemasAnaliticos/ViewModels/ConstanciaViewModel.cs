namespace SistemasAnaliticos.ViewModels
{
    public class ConstanciaViewModel
    {
        public long idConstancia { get; set; }
        public DateTime? fechaPedido { get; set; }
        public string nombreEmpleado { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
        public string dirijido { get; set; }
        public DateTime? fechaRequerida { get; set; }
        public string comentarios { get; set; }
        public string nombreArchivo { get; set; }
        public string tipoMIME { get; set; }
        public long? tamanoArchivo { get; set; }
    }
}
