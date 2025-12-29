using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.ViewModels
{
    public class CambiarContrasenaViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._-])[A-Za-z\d@$!%*?&#._-]{8,}$",
            ErrorMessage = "La contraseña no cumple con los requisitos de seguridad"
        )]
        public string NuevaContrasena { get; set; }
    }
}
