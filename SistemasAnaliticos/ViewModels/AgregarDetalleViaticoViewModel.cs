using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class AgregarDetalleViaticoViewModel
    {
        public long IdViatico { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateOnly Fecha { get; set; }

        [Required(ErrorMessage = "El tipo es requerido")]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El detalle es requerido")]
        [MaxLength(500, ErrorMessage = "El detalle no puede exceder 500 caracteres")]
        public string Detalle { get; set; }
    }
}
