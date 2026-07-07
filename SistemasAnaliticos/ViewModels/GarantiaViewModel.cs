using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class GarantiaViewModel
    {
        public long idGarantia { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string nombreEmpleado { get; set; }
        public string departamento { get; set; }
        public string moneda { get; set; }
        public decimal monto { get; set; }
        public string aFavorDe { get; set; }
        public string nombreLicitacion { get; set; }
        public bool prorroga { get; set; }
        public string? numeroGarantia { get; set; }
        public string? numeroLicitacion { get; set; }

        public string tipoLicitacion { get; set; } // LY, LE, LD, PX
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFinalizacion { get; set; }
        public string plazo { get; set; }
        public string? observacion { get; set; }

        // PASO 3 - Documentos Adjuntos
        public List<IFormFile> adjuntos { get; set; }

        public string estado { get; set; }
    }
}
