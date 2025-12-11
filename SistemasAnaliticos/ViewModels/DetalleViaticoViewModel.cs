namespace SistemasAnaliticos.ViewModels
{
    public class DetalleViaticoViewModel
    {
        public long idViaticoDetalle { get; set; }
        public DateOnly fecha { get; set; }
        public string tipo { get; set; }
        public decimal monto { get; set; }
        public string detalle { get; set; }

        // Color por tipo
        public string TipoColor => tipo switch
        {
            "Alimentación" => "success",
            "Transporte" => "primary",
            "Combustible" => "warning",
            "Kilometraje" => "info",
            "Hospedaje" => "dark",
            _ => "secondary"
        };
    }
}
