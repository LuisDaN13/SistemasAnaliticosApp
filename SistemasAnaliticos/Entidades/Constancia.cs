using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Constancia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idConstancia { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public DateOnly fechaCreacion { get; set; }        

        [Required]
        public string nombreEmpleado { get; set; }

        [Required]
        public string departamento { get; set; }

        [Required]
        [StringLength(30)]
        public string tipo { get; set; }

        [StringLength(100)]
        public string? dirijido { get; set; }

        [DataType(DataType.Date)]
        public DateTime? fechaRequerida { get; set; }

        public string? comentarios { get; set; }

        [NotMapped]
        public IFormFile fotoFile { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos { get; set; }
        public string? nombreArchivo { get; set; }
        public string? tipoMIME { get; set; }
        public long? tamanoArchivo { get; set; }

        [Required]
        public string estado { get; set; }
    }
}
