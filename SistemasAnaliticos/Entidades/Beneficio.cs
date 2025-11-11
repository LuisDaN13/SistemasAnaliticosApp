using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Beneficio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idBeneficio { get; set; }
        public DateTime fechaPedido { get; set; }
        public string nombrePersona { get; set; }

        [StringLength(30)]
        public string tipo { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal monto { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal restante { get; set; }
        public string Comentarios { get; set; }

        [Required]
        public string estado { get; set; }
    }
}
