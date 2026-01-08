using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class ExtrasDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idExtrasDetalle { get; set; }

        public long idExtra { get; set; }

        [Required]
        public DateOnly fecha { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? horaInicio { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? horaFin { get; set; }

        [Required]
        public string detalle { get; set; }
        public string? atm { get; set; }
        public string? noCaso { get; set; }
        public string? noBoleta { get; set; }

        public string lugar { get; set; }
        public string sucursal { get; set; }

        public Extras Extra { get; set; }
    }
}
