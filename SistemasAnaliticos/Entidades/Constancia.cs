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

        [DataType(DataType.Date)]
        public DateTime? fechaRequerida { get; set; }
        public string? Comentarios { get; set; }


        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos { get; set; }
        public string? nombreArchivo { get; set; }
        public string? tipoMIME { get; set; }
        public long? tamanoArchivo { get; set; }

        [Required]
        public string estado { get; set; }
    }
}
