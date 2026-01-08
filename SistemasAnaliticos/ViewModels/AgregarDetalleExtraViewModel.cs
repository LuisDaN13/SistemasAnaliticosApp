using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class AgregarDetalleExtraViewModel
    {
        public long idExtra { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateOnly fecha { get; set; }

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        public TimeSpan horaInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es requerida")]
        public TimeSpan horaFin { get; set; }

        [Required]
        [StringLength(500)]
        public string detalle { get; set; }

        public string? atm { get; set; }
        public string? noCaso { get; set; }
        public string? noBoleta { get; set; }
        public string lugar { get; set; }
        public string sucursal { get; set; }
        public int totalHoras { get; set; }
    }
}
