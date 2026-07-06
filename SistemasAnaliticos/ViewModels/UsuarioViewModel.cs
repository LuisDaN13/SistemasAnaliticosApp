namespace SistemasAnaliticos.ViewModels
{
    public class UsuarioViewModel
    {
        public string Id { get; set; }
        public string NombreCompleto { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public string RolNombre => Roles != null && Roles.Any()
            ? string.Join(", ", Roles)
            : "Sin rol asignado";
        public bool Estado { get; set; }
    }
}
