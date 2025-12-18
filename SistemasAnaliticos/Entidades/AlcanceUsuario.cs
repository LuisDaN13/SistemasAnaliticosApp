using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.Entidades
{
    public class AlcanceUsuario
    {
        [Key]
        public int idAlcance { get; set; }

        // FK a AspNetRoles
        [Required]
        public string rolId { get; set; }

        // Global | Subordinados | Propio
        [Required]
        [MaxLength(50)]
        public string alcance { get; set; }
    }
}
