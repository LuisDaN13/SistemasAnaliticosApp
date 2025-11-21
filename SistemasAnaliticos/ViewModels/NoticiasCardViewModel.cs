namespace SistemasAnaliticos.ViewModels
{
    public class NoticiasCardsViewModel
    {
        public long Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public byte[]? Foto { get; set; }
        public bool Estado { get; set; }
    }
}
