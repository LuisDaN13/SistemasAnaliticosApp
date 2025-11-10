using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.DTO
{
    public class PermisoDTO
    {
        public List<Permiso> Incapacidades { get; set; }
        public List<Permiso> Citas { get; set; }
        public List<Permiso> Vacaciones { get; set; }
        public List<Permiso> Teletrabajo { get; set; }
        public List<Permiso> Especial { get; set; }

        public List<Constancia> Laboral { get; set; }
        public List<Constancia> Salarial { get; set; }
    }
}
