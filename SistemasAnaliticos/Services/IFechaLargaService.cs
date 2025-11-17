namespace SistemasAnaliticos.Services
{
    public interface IFechaLargaService
    {
        string FechaEnPalabras(DateOnly fecha);
        string NumeroEnPalabras(int numero);
        string SalarioEnPalabras(int numero);
    }
}
