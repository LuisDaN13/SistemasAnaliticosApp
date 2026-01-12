using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Fotos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idFoto { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[]? foto { get; set; }

        [NotMapped]
        [MaxFileSize(5)]
        public IFormFile? fotoFile { get; set; } = null!;
        
        public bool estado { get; set; }
    }
}
