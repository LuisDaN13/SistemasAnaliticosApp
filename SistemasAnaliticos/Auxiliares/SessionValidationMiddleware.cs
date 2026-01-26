using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Auxiliares
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _timeout;
        private readonly List<PathString> _excludedPaths;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SessionValidationMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SessionValidationMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<SessionValidationMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
            _scopeFactory = scopeFactory;

            // Valores por defecto; considera mover a IOptions si quieres configurarlo.
            _timeout = TimeSpan.FromMinutes(40);
            _excludedPaths = new List<PathString>
            {
                new PathString("/Usuario/Login"),
                new PathString("/Usuario/Logout"),
                new PathString("/css"),
                new PathString("/js"),
                new PathString("/lib"),
                new PathString("/images"),
                new PathString("/api"),
                new PathString("/favicon.ico")
            };
        }

        public async Task InvokeAsync(HttpContext context, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            // 1. Excluir rutas que no necesitan validación
            if (ShouldSkipValidation(context.Request.Path))
            {
                _logger.LogDebug("SessionValidation: ruta excluida {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = userManager.GetUserId(context.User);
                _logger.LogDebug("SessionValidation: usuario autenticado, userId={UserId}", userId);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("SessionValidation: userId vacío, forzando signout");
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                var cacheKey = $"UserSession_{userId}";

                // 2. Obtener user directamente desde BD para validar SessionId (evita problemas por cache obsoleta)
                Usuario? userSession = null;
                try
                {
                    userSession = await userManager.FindByIdAsync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SessionValidation: error al buscar usuario en BD {UserId}", userId);
                }

                if (userSession == null)
                {
                    _logger.LogInformation("SessionValidation: Usuario no existe en BD: {UserId}", userId);
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // 3. Validar SessionId (claim vs BD)
                var claimSessionId = context.User.FindFirst("SessionId")?.Value;
                _logger.LogDebug("SessionValidation: claimSessionId={Claim}, dbSessionId={Db}", claimSessionId ?? "<null>", userSession.sessionId ?? "<null>");

                if (string.IsNullOrEmpty(claimSessionId) || userSession.sessionId != claimSessionId)
                {
                    _logger.LogInformation("SessionValidation: SessionId inválida para {UserId} (claim:{Claim}, db:{Db})", userId, claimSessionId, userSession.sessionId);
                    // asegurar limpieza de cache relacionada
                    try
                    {
                        _cache.Remove(cacheKey);
                        _cache.Remove($"LastActivity_{userId}");
                        _cache.Remove($"DbUpdate_{userId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SessionValidation: fallo al limpiar cache para {UserId}", userId);
                    }

                    // Forzar eliminación de cookie en la respuesta + signout del manager
                    try
                    {
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SessionValidation: context.SignOutAsync falló para {UserId}", userId);
                    }

                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // 4. Control de inactividad: preferir cache para checks rápidos
                var lastActivityKey = $"LastActivity_{userId}";
                if (!_cache.TryGetValue<DateTime?>(lastActivityKey, out var lastActivity))
                {
                    lastActivity = userSession.lastActivityUtc ?? DateTime.MinValue;
                }

                var last = lastActivity.GetValueOrDefault(DateTime.MinValue);
                _logger.LogDebug("SessionValidation: lastActivityUtc={Last} now={Now}", last, DateTime.UtcNow);

                if (DateTime.UtcNow - last > _timeout)
                {
                    _logger.LogInformation("SessionValidation: Inactividad excedida para {UserId}", userId);
                    try
                    {
                        _cache.Remove(cacheKey);
                        _cache.Remove(lastActivityKey);
                        _cache.Remove($"DbUpdate_{userId}");
                    }
                    catch { }

                    try
                    {
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                    }
                    catch { }

                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // 5. Actualizar cache de última actividad (rápido)
                _cache.Set(lastActivityKey, DateTime.UtcNow, TimeSpan.FromMinutes(10));

                // 6. Actualizar BD de forma segura: crear scope nuevo dentro del background task
                var dbUpdateKey = $"DbUpdate_{userId}";
                if (!_cache.TryGetValue(dbUpdateKey, out _))
                {
                    // Crear tarea en background pero con scope propio
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
                            var u = await scopedUserManager.FindByIdAsync(userId);
                            if (u != null)
                            {
                                u.lastActivityUtc = DateTime.UtcNow;
                                await scopedUserManager.UpdateAsync(u);
                                // Actualizar cache solo de última actividad para coherencia
                                _cache.Set(lastActivityKey, DateTime.UtcNow, TimeSpan.FromMinutes(10));
                            }
                            _cache.Set(dbUpdateKey, true, TimeSpan.FromSeconds(30));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al actualizar lastActivity en BD para {UserId}", userId);
                        }
                    });
                }
            }
            else
            {
                _logger.LogDebug("SessionValidation: request no autenticado para path {Path}", context.Request.Path);
            }

            await _next(context);
        }

        private bool ShouldSkipValidation(PathString path)
        {
            if (path.HasValue)
            {
                foreach (var excluded in _excludedPaths)
                {
                    if (path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task SignOutAndRedirect(HttpContext context, SignInManager<Usuario> signInManager)
        {
            _logger.LogInformation("SessionValidation: realizando SignOut y redirigiendo a /Usuario/Login para path {Path}", context.Request.Path);

            // Forzar cierre de cookie del esquema de Identity
            try
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SessionValidation: context.SignOutAsync falló");
            }

            try
            {
                await signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SessionValidation: signInManager.SignOutAsync falló");
            }

            // Si es request de API devolvemos 401 y no redirigimos
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            context.Response.Redirect("/Usuario/Login");
        }
    }

    public static class SessionValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder app) =>
            app.UseMiddleware<SessionValidationMiddleware>();
    }
}