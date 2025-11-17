namespace SistemasAnaliticos.Services
{
    public interface IConstanciaService
    {
        Task<byte[]> GenerarConstanciaLaboral(string nombrePersona, string cedula, string departamento, DateTime? fechaIngreso, string puesto, string dirjido);
        Task<byte[]> GenerarConstanciaSalarial(string nombrePersona, string cedula, string departamento, DateTime? fechaIngreso, string puesto, int salarioBruto, int deducciones, int salarioNeto);

    }
}
