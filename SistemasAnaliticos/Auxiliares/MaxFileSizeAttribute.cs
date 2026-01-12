using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.Auxiliares
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSizeInBytes;
        public MaxFileSizeAttribute(int maxFileSizeInMB)
        {
            _maxFileSizeInBytes = maxFileSizeInMB * 1024 * 1024; // Convertir MB a Bytes
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null && file.Length > _maxFileSizeInBytes)
            {
                return new ValidationResult($"El archivo no puede superar los {_maxFileSizeInBytes / (1024 * 1024)} MB.");
            }

            return ValidationResult.Success;
        }
    }
}