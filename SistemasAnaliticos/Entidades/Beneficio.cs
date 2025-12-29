using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Beneficio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idBeneficio { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public DateOnly fechaCreacion { get; set; }

        [Required]
        public string nombreEmpleado { get; set; }

        [Required]
        public string departamento { get; set; }

        [Required]
        [StringLength(30)]
        public string tipo { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal monto { get; set; }
        public string comentarios { get; set; }

        [Required]
        public string estado { get; set; }
    }
}
