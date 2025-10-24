namespace SistemasAnaliticos.DTO
{
    public class CardsDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public string CorreoEmp { get; set; }
        public string TelefonoEmp { get; set; }
        public string CorreoPerso { get; set; }
        public string TelefonoPerso { get; set; }
        public bool Estado { get; set; }
        public byte[]? Foto { get; set; }
    }
}
