using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class CambiarContrasenaViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NuevaContrasena { get; set; }
    }
}
