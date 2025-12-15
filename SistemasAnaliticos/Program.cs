using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemasAnaliticos.Auxiliares;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IConstanciaService, ConstanciaService>();
builder.Services.AddScoped<IFechaLargaService, FechaLargaService>();
builder.Services.AddScoped<IRolPermisoService, RolPermisoService>();

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

// Servicio de Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(180);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();