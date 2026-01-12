namespace SistemasAnaliticos.Auxiliares
{
    public class ProcesarAdjuntos
    {
        public async Task<byte[]?> ProcesarArchivoAdjunto(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await archivo.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
