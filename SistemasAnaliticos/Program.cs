using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Middlewares;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ConfiguracionEmail>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddControllersWithViews();
//builder.Services.AddControllersWithViews(options =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();

//    options.Filters.Add(new AuthorizeFilter(policy));
//});
builder.Services.AddScoped<IConstanciaService, ConstanciaService>();
builder.Services.AddScoped<IFechaLargaService, FechaLargaService>();
builder.Services.AddScoped<IAlcanceUsuarioService, AlcanceUsuarioService>();
builder.Services.AddScoped<IRolPermisoService, RolPermisoService>();
builder.Services.AddScoped<IPermisoAlcanceService, PermisoAlcanceService>();
builder.Services.AddScoped<IAuthorizationHandler, PermisoHandler>();
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in PermisosSistema.Todos)
    {
        options.AddPolicy(permiso, policy =>
            policy.Requirements.Add(new PermisoRequirement(permiso)));
    }
});

// Base de Datos a Usar
builder.Services.AddDbContext<DBContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("pruebas")));

// Servicio de Identity
builder.Services.AddIdentity<Usuario, Rol>(o =>
{
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequiredLength = 8;
    o.Password.RequireUppercase = false;
    o.Password.RequireLowercase = false;
    o.User.RequireUniqueEmail = true;
    o.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Usuario/Login";
    options.AccessDeniedPath = "/Usuario/AccesoDenegado";
});

// Servicio de Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(180);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Expiración por inactividad: 1 minuto, con sliding expiration
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
    options.SlidingExpiration = true;
    options.LoginPath = "/Usuario/Login"; // ajustar si la ruta es diferente
});

// Aplicacion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseSessionValidation();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();