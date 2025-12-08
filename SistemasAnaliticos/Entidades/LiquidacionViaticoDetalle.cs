using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class LiquidacionViaticoDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idViaticoDetalle { get; set; }

        public long idViatico { get; set; }

        [Required]
        public DateOnly fecha { get; set; }

        [Required]
        public string tipo { get; set; }

        [Required]
        public decimal monto { get; set; }

        [Required]
        public string detalle { get; set; }

        public LiquidacionViatico Liquidacion { get; set; }
    }
}
