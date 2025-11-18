using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Services;
using Spire.Doc;
using System.Runtime.InteropServices;

public class ConstanciaService : IConstanciaService
{
    private readonly IFechaLargaService _fechaLargaService;


    public ConstanciaService(IFechaLargaService fechaLargaService)
    {
        _fechaLargaService = fechaLargaService;
    }

    public async Task<byte[]> GenerarConstanciaLaboral(
        string nombrePersona,
        string cedula,
        string departamento,
        DateTime? fechaIngreso,
        string puesto,
        string dirijido)
    {
        try
        {
            string templatePath = "wwwroot/word/Laboral.docx";
            string fechaIngresoTexto = $"{fechaIngreso?.Day} de {fechaIngreso?.ToString("MMMM")} del año {fechaIngreso?.Year}";
            string cedulaFormateada = $"{cedula[0]}-{cedula.Substring(1, 4)}-{cedula.Substring(5, 4)}";
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"
                : "America/Costa_Rica";

            DateOnly hoy = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId))
            );

            // Crear documento Word
            Document doc = new Document();
            doc.LoadFromFile(templatePath);

            // Reemplazar los placeholders
            doc.Replace("dirijido", dirijido, false, true);
            doc.Replace("nombrePersona", nombrePersona, false, true);
            doc.Replace("cedula", cedulaFormateada, false, true);
            doc.Replace("departamento", departamento, false, true);
            doc.Replace("fechaIngresoEmpleado", fechaIngresoTexto, false, true);
            doc.Replace("puestoEmpleado", puesto, false, true);
            doc.Replace("fechaHoy", _fechaLargaService.FechaEnPalabras(hoy), false, true);

            // Convertir directamente a PDF y retornar como byte[]
            using (var pdfStream = new MemoryStream())
            {
                doc.SaveToStream(pdfStream, FileFormat.PDF);
                return pdfStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar la constancia: {ex.Message}", ex);
        }
    }
    public async Task<byte[]> GenerarConstanciaSalarial(
        string nombrePersona,
        string cedula,
        string departamento,
        DateTime? fechaIngreso,
        string puesto,
        int salarioBruto,
        int deducciones,
        int salarioNeto)
    {
        try
        {
            string templatePath = "wwwroot/word/Salarial.docx";
            string fechaIngresoTexto = $"{fechaIngreso?.Day} de {fechaIngreso?.ToString("MMMM")} del año {fechaIngreso?.Year}";
            string cedulaFormateada = cedula.Length == 9 ? $"{cedula[0]}-{cedula.Substring(1, 4)}-{cedula.Substring(5, 4)}" : cedula;
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"
                : "America/Costa_Rica";

            DateOnly hoy = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId))
            );

            // Crear documento Word
            Document doc = new Document();
            doc.LoadFromFile(templatePath);

            // Reemplazar los placeholders
            doc.Replace("nombrePersona", nombrePersona, false, true);
            doc.Replace("cedula", cedulaFormateada, false, true);
            doc.Replace("departamento", departamento, false, true);
            doc.Replace("fechaIngresoEmpleado", fechaIngresoTexto, false, true);
            doc.Replace("puestoEmpleado", puesto, false, true);

            doc.Replace("salarioBruto", salarioBruto.ToString("C"), false, true);
            doc.Replace("numeroLargoSalarioBruto", $"{_fechaLargaService.SalarioEnPalabras(salarioBruto)} colones", false, true);

            doc.Replace("deduccionesFijas", deducciones.ToString("C"), false, true);
            doc.Replace("numeroLargoDeducciones", $"{_fechaLargaService.SalarioEnPalabras(deducciones)} colones", false, true);

            doc.Replace("salarioNeto", salarioNeto.ToString("C"), false, true);
            doc.Replace("numeroLargoSalarioNeto", $"{_fechaLargaService.SalarioEnPalabras(salarioNeto)} colones", false, true);

            doc.Replace("fechaHoy", _fechaLargaService.FechaEnPalabras(hoy), false, true);

            // Convertir directamente a PDF y retornar como byte[]
            using (var pdfStream = new MemoryStream())
            {
                doc.SaveToStream(pdfStream, FileFormat.PDF);
                return pdfStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar la constancia: {ex.Message}", ex);
        }
    }
}