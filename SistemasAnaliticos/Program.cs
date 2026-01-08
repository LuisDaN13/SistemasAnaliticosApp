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
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
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

// Servicios esenciales
builder.Services.Configure<ConfiguracionEmail>(builder.Configuration.GetSection("Smtp"));
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("ambiente")));

var app = builder.Build();

// Pipeline mínimo
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();