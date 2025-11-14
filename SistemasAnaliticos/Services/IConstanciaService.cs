using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;

namespace SistemasAnaliticos.Services
{
    public interface IConstanciaService
    {
        Task<byte[]> GenerarConstanciaLaboral(string nombrePersona, string cedula, string departamento, string puesto);

    }
}
