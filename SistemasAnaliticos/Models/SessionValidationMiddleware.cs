using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SistemasAnaliticos.Entidades;
using System.Security.Claims;

namespace SistemasAnaliticos.Middlewares
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Usuario/Login");
                    return;
                }

                // obtener sessionId de la claim
                var claimSessionId = context.User.FindFirst("SessionId")?.Value;
                if (string.IsNullOrEmpty(claimSessionId) || user.sessionId != claimSessionId)
                {
                    // otra sesión se creó o claim faltante -> cerrar sesión
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Usuario/Login");
                    return;
                }

                // comprobar inactividad
                var last = user.lastActivityUtc ?? DateTime.MinValue;
                if (DateTime.UtcNow - last > _timeout)
                {
                    // tiempo de inactividad excedido
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Usuario/Login");
                    return;
                }

                // actualizar última actividad
                user.lastActivityUtc = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }

            await _next(context);
        }
    }

    public static class SessionValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder app) =>
            app.UseMiddleware<SessionValidationMiddleware>();
    }
}