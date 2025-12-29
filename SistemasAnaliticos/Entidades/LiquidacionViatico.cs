using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class LiquidacionViatico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idViatico { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public DateOnly fechaCreacion { get; set; }

        [Required]
        public string nombreEmpleado { get; set; }

        [Required]
        public string departamento { get; set; }

        [Required]
        public string jefe { get; set; }

        public decimal total { get; set; }

        [Required]
        public string estado { get; set; }

        public List<LiquidacionViaticoDetalle> Detalles { get; set; } = new();
    }
}
