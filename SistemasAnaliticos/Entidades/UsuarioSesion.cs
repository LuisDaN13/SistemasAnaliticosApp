using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class UsuarioSesion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string SessionId { get; set; }

        [Required]
        public DateTime LoginDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property SIN virtual
        [ForeignKey("UserId")]
        public Usuario User { get; set; }
    }
}
