using SistemasAnaliticos.Auxiliares;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Garantia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idGarantia { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public DateTime fechaCreacion { get; set; }

        [Required]
        public string nombreEmpleado { get; set; }

        [Required]
        public string departamento { get; set; }

        [Required]
        public string moneda { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? monto { get; set; }

        [Required]
        public string aFavorDe { get; set; }

        [Required]
        public string nombreLicitacion { get; set; }

        [Required]
        public bool prorroga { get; set; }

        public string? numeroGarantia { get; set; }
        public string? numeroLicitacion { get; set; }

        [Required]
        public string tipoLicitacion { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? fechaInicio { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? fechaFinalizacion { get; set; }

        [Required]
        public string plazo { get; set; }
        public string? observacion { get; set; }
        public string estado { get; set; }


        [NotMapped]
        [MaxFileSize(10)]
        public IFormFile? adjuntoFile1 { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos1 { get; set; }
        public string? nombreArchivo1 { get; set; }
        public string? tipoMIME1 { get; set; }
        public long? tamanoArchivo1 { get; set; }

        [NotMapped]
        [MaxFileSize(10)]
        public IFormFile? adjuntoFile2 { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos2 { get; set; }
        public string? nombreArchivo2 { get; set; }
        public string? tipoMIME2 { get; set; }
        public long? tamanoArchivo2 { get; set; }

        [NotMapped]
        [MaxFileSize(10)]
        public IFormFile? adjuntoFile3 { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos3 { get; set; }
        public string? nombreArchivo3 { get; set; }
        public string? tipoMIME3 { get; set; }
        public long? tamanoArchivo3 { get; set; }

        [NotMapped]
        [MaxFileSize(10)]
        public IFormFile? adjuntoFile4 { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos4 { get; set; }
        public string? nombreArchivo4 { get; set; }
        public string? tipoMIME4 { get; set; }
        public long? tamanoArchivo4 { get; set; }

        [NotMapped]
        [MaxFileSize(10)]
        public IFormFile? adjuntoFile5 { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos5 { get; set; }
        public string? nombreArchivo5 { get; set; }
        public string? tipoMIME5 { get; set; }
        public long? tamanoArchivo5 { get; set; }
    }
}


