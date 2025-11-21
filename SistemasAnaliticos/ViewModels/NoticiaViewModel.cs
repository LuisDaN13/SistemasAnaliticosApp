namespace SistemasAnaliticos.ViewModels
{
    public class NoticiaViewModel
    {
        public long idNoticia { get; set; }
        public string titulo { get; set; }
        public string categoria { get; set; }
        public byte[]? foto { get; set; }

        public DateOnly fechaPublicacion { get; set; }
        public TimeSpan horaPublicacion { get; set; }
        public string autor { get; set; }

        public string contenidoTexto { get; set; }
    }
}
