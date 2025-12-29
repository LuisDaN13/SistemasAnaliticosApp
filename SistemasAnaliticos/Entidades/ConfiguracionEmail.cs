namespace SistemasAnaliticos.Entidades
{
    public sealed class ConfiguracionEmail
    {
        // Campos originales (se mantienen por compatibilidad)
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FromName { get; init; } = string.Empty;
        public string FromEmail { get; init; } = string.Empty;

        // NUEVOS campos para Graph API
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
        public string TenantId { get; init; } = string.Empty;
    }
}