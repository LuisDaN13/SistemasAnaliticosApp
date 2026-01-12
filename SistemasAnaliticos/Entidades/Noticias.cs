using SistemasAnaliticos.Auxiliares;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Noticias
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idNoticia { get; set; }
        public string titulo { get; set; } = null!;
        public string categoria { get; set; } = null!;

        public DateOnly fechaPublicacion { get; set; }
        public TimeSpan horaPublicacion { get; set; }
        public string autor { get; set; } = null!;
        public string departamento { get; set; } = null!;

        [Column(TypeName = "varchar(max)")]
        public string? contenidoTexto { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[]? foto { get; set; }

        [NotMapped]
        [MaxFileSize(5)]
        public IFormFile? fotoFile { get; set; } = null!;

        public bool estado { get; set; }
    }
}
