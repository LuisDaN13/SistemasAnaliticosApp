using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Extras
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idExtra { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public DateOnly fechaCreacion { get; set; }

        [Required]
        public string nombreEmpleado { get; set; }

        [Required]
        public string departamento { get; set; }

        [Required]
        public string tipoExtra { get; set; }

        public string jefe { get; set; }

        public decimal totalHoras { get; set; }


        [Required]
        public string estado { get; set; }


        public List<ExtrasDetalle> Detalles { get; set; } = new();
    }
}
