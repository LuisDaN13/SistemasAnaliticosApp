using System.Text;
using System.Text.RegularExpressions;

namespace SistemasAnaliticos.Models
{
    public static class NormalizarApellido
    {
        public static string normalizar(this string apellido)
        {
            if (string.IsNullOrEmpty(apellido))
                return apellido;

            string normalized = apellido.Normalize(NormalizationForm.FormD);
            normalized = Regex.Replace(normalized, @"[^\w\s]", "");
            return normalized.ToLower().Trim();
        }
    }
}