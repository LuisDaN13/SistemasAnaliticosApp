using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Auxiliares
{
    public class codigoFotos
    {
        public class CodigoFotos
        {
            public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return null;

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }

            public async Task<byte[]> ProcesarArchivo(IFormFile nuevoArchivo, byte[] archivoAnterior)
            {
                if (nuevoArchivo != null && nuevoArchivo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await nuevoArchivo.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }

                return archivoAnterior;
            }
        }
    }
}
