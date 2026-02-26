using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class GarantiaRegistroViewModel
    {
        // PASO 1 - Información General
        public decimal? Monto { get; set; }
        public char Moneda { get; set; } // CRC o USD
        public string AFavorDe { get; set; }
        public string NombreLicitacion { get; set; }
        public bool Prorroga { get; set; }
        public string NumeroGarantia { get; set; }
        public string NumeroLicitacion1 { get; set; }

        // PASO 2 - Datos de Licitación
        public string TipoLicitacion { get; set; } // LY, LE, LD, PX
        public string NumeroLicitacion2 { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Plazo { get; set; }
        public string Observacion { get; set; }

        // PASO 3 - Documentos Adjuntos
        public List<IFormFile> Adjuntos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreEmpleado { get; set; }
        public string UsuarioId { get; set; }
    }
}
