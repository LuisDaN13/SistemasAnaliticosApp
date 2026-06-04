namespace SistemasAnaliticos.ViewModels
{
    public class OrganigramaVM
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Puesto { get; set; } = "";
        public string Departamento { get; set; } = "";
        public string CorreoEmpresa { get; set; } = "";
        public string? JefeId { get; set; }
    }
}
