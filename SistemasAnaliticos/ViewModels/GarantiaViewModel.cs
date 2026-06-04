using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class GarantiaViewModel
    {
        public long idGarantia { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string Moneda { get; set; }
        public decimal? Monto { get; set; }
        public decimal? porcentaje { get; set; }
        public decimal montoFinal { get; set; }
        public string AFavorDe { get; set; }
        public string NombreLicitacion { get; set; }
        public bool Prorroga { get; set; }
        public string? NumeroGarantia { get; set; }
        public string? NumeroLicitacion { get; set; }

        public string TipoLicitacion { get; set; } // LY, LE, LD, PX
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Plazo { get; set; }
        public string? Observacion { get; set; }

        // PASO 3 - Documentos Adjuntos
        public List<IFormFile> Adjuntos { get; set; }

        public string estado { get; set; }
    }
}
