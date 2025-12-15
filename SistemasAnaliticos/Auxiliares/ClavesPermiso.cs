namespace SistemasAnaliticos.Auxiliares
{
    public static class ClavesPermiso
    {
        public static string Crear(string modulo, string accion)
        {
            return $"{modulo}.{accion}";
        }
    }
}
