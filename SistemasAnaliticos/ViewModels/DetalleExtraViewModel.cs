using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class DetalleExtraViewModel
    {
        public long idExtraDetalle { get; set; }
        public DateOnly fecha { get; set; }

        public TimeSpan? horaInicio { get; set; }
        public TimeSpan? horaFin { get; set; }

        public string detalle { get; set; }
        public string atm { get; set; }
        public string noCaso { get; set; }
        public string noBoleta { get; set; }

        public string lugar { get; set; }
        public string sucursal { get; set; }


        // Color por tipo
        public string TipoColor => lugar switch
        {
            "BN" => "success",
            "BCR" => "primary",
            "BCT" => "warning",
            "Goblal Exchange" => "info",
            "Mutual Alajuela" => "dark",
            "SASA" => "#d90100",
            _ => "secondary"
        };
    }
}
