using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ========== SOLUCIÓN SIMPLE PARA IIS ==========
// ELIMINA completamente PersistKeysToFileSystem
// Deja que ASP.NET Core maneje DataProtection automáticamente
builder.Services.AddDataProtection()
    .SetApplicationName("SistemasAnaliticos");
// FIN DE LA SOLUCIÓN

// ========== CONFIGURACIÓN BÁSICA ==========
builder.Services.AddIdentity<Usuario, Rol>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DBContext>()
.AddDefaultTokenProviders();

// Configuración básica de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.LoginPath = "/Usuario/Login";
    options.LogoutPath = "/Usuario/LogOut";
    options.AccessDeniedPath = "/Usuario/AccesoDenegado";
});

// Controladores con auth global
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter(
        new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build()));
});

// Agregá después de AddControllersWithViews
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Servicios esenciales
builder.Services.AddMemoryCache();
builder.Services.Configure<ConfiguracionEmail>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IConstanciaService, ConstanciaService>();
builder.Services.AddScoped<IFechaLargaService, FechaLargaService>();
builder.Services.AddScoped<IAlcanceUsuarioService, AlcanceUsuarioService>();
builder.Services.AddScoped<IRolPermisoService, RolPermisoService>();
builder.Services.AddScoped<IPermisoAlcanceService, PermisoAlcanceService>();
builder.Services.AddScoped<IAuthorizationHandler, PermisoHandler>();

// Permisos
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in PermisosSistema.Todos)
    {
        options.AddPolicy(permiso, policy =>
            policy.Requirements.Add(new PermisoRequirement(permiso)));
    }
});

// Base de datos (simple)
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("pruebas")));

var app = builder.Build();

// Pipeline mínimo
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    // Genera un nonce único por cada request
    var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    context.Items["CSPNonce"] = nonce;

    var headers = context.Response.Headers;

    var csp =
        $"default-src 'self'; " +
        $"script-src 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net https://code.jquery.com https://cdnjs.cloudflare.com https://cdn.datatables.net https://unpkg.com; " +
        $"script-src-elem 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net https://code.jquery.com https://cdnjs.cloudflare.com https://cdn.datatables.net https://unpkg.com; " +
        $"style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com https://cdn.datatables.net https://cdnjs.cloudflare.com; " +
        $"style-src-elem 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com https://cdn.datatables.net https://cdnjs.cloudflare.com; " +
        $"img-src 'self' data: https://cdn.datatables.net; " +
        $"font-src 'self' https://cdn.jsdelivr.net https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
        $"frame-ancestors 'none'; " +
        $"base-uri 'self'; " +
        $"form-action 'self'; " +
        (app.Environment.IsDevelopment()
            ? "connect-src 'self' wss://localhost:*;"
            : "connect-src 'self';");

    headers["Content-Security-Policy"] = csp;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    headers["Cross-Origin-Resource-Policy"] = "same-origin";

    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSessionValidation();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();