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
            _timeout = TimeSpan.FromMinutes(1);
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
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = userManager.GetUserId(context.User);
                if (string.IsNullOrEmpty(userId))
                {
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                var cacheKey = $"UserSession_{userId}";

                // 2. Obtener user desde cache o BD (usar FindByIdAsync para evitar depender de ClaimsPrincipal internamente)
                var userSession = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                    var u = await userManager.FindByIdAsync(userId);
                    return u;
                });

                if (userSession == null)
                {
                    _logger.LogInformation("Usuario no existe en BD: {UserId}", userId);
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // 3. Validar SessionId (claim vs BD)
                var claimSessionId = context.User.FindFirst("SessionId")?.Value;
                if (string.IsNullOrEmpty(claimSessionId) || userSession.sessionId != claimSessionId)
                {
                    _logger.LogInformation("SessionId inválida para {UserId}", userId);
                    _cache.Remove(cacheKey);
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
                if (DateTime.UtcNow - last > _timeout)
                {
                    _logger.LogInformation("Inactividad excedida para {UserId}", userId);
                    _cache.Remove(cacheKey);
                    _cache.Remove(lastActivityKey);
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
                                // Actualizar también el cache de sesión para mantener coherencia
                                _cache.Set(cacheKey, u, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });
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
            await signInManager.SignOutAsync();

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