using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Constancia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idConstancia { get; set; }
        public DateTime fechaPedido { get; set; }        
        public string nombrePersona { get; set; }

        [StringLength(30)]
        public string tipo { get; set; }

        [StringLength(100)]
        public string dirijido { get; set; }
        public string Comentarios { get; set; }
    }
}
