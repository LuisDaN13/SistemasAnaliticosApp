using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Services;

public class ConstanciaService : IConstanciaService
{
    public async Task<byte[]> GenerarConstanciaLaboral(
        string nombrePersona,
        string cedula,
        string departamento,
        string puesto)
    {
        try
        {
            string templatePath = "wwwroot/pdf/Constancia.pdf";

            using var memoryStream = new MemoryStream();

            var pdfReader = new PdfReader(templatePath);
            var pdfWriter = new PdfWriter(memoryStream);
            var pdfDoc = new PdfDocument(pdfReader, pdfWriter);
            var document = new Document(pdfDoc);

            // -------------------------------------------
            // ÁREA DE TEXTO MÁXIMA (ajústalo si querés)
            // -------------------------------------------
            float X = 80;
            float Y = 70;
            float PAGE_WIDTH = 595;

            float WIDTH = PAGE_WIDTH - (X + Y);


            // -------------------------------------------
            // 1) ENCABEZADO (Señores / A quien corresponda / Presente)
            // -------------------------------------------
            var destino = new Paragraph()
                .SetFontSize(11)
                .Add("Señores\nA quien corresponda\nPresente");

            document.Add(
                destino.SetFixedPosition(1, X, 578, WIDTH)
            );

            // -------------------------------------------
            // 2) PÁRRAFO PRINCIPAL
            // -------------------------------------------
            var p1 = new Paragraph()
                .SetFontSize(11)
                .Add("Por este medio hago constar que, ")
                .Add(new Text(nombrePersona))
                .Add(", portador de la cédula de identidad N° ")
                .Add(new Text(cedula))
                .Add(", labora para la compañía Sistemas Analíticos S. A. con cédula jurídica N° ")
                .Add(new Text("3-101-123456"))
                .Add(", en el área de ")
                .Add(new Text(departamento))
                .Add(", desde el ")
                .Add(new Text("05/05/2025"))
                //.Add(new Text(model.fechaIngresoEmpleado?.ToString("dd/MM/yyyy")).SetBold())
                .Add(" cumpliendo formalmente su quinquenio.");

            document.Add(
                p1.SetFixedPosition(1, X, 500, WIDTH)
            );

            // -------------------------------------------
            // 3) PÁRRAFO DE PUESTO
            // -------------------------------------------
            var p2 = new Paragraph()
                .SetFontSize(11)
                .Add("Actualmente es personal activo de nuestra empresa. Desempeña el puesto de ")
                .Add(new Text(puesto))
                .Add(".");

            document.Add(
                p2.SetFixedPosition(1, X, 370, WIDTH)
            );

            // -------------------------------------------
            // 4) PÁRRAFO DE FECHA DE EMISIÓN
            // -------------------------------------------
            DateTime fechaCR = DateTime.Now;

            var p3 = new Paragraph()
                .SetFontSize(11)
                .Add("Se extiende a solicitud del interesado a los ")
                .Add(new Text(fechaCR.Day.ToString()))
                .Add(" días del mes de ")
                .Add(new Text(fechaCR.ToString("MMMM")))
                .Add(" del ")
                .Add(new Text(fechaCR.Year.ToString()))
                .Add(".");

            document.Add(
                p3.SetFixedPosition(1, X, 270, WIDTH)
            );

            // -------------------------------------------
            // 5) "Atentamente"
            // -------------------------------------------
            var at = new Paragraph("Atentamente,")
                .SetFontSize(11);

            document.Add(
                at.SetFixedPosition(1, X, 170, WIDTH)
            );

            // -------------------------------------------
            // 6) FIRMA
            // -------------------------------------------
            var firma = new Paragraph()
                .SetFontSize(11)
                .Add("_______________________________\n")
                .Add("Recursos Humanos\n")
                .Add("Sistemas Analíticos, S.A.");

            document.Add(
                firma.SetFixedPosition(1, X, 140, WIDTH)
            );

            document.Close();

            // Retornar los bytes del PDF
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar la constancia: {ex.Message}", ex);
        }
    }
}