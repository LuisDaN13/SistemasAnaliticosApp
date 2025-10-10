using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Base de Datos a Usar
var con = builder.Configuration.GetConnectionString("pruebas");
builder.Services.AddDbContext<DBContext>( o => o.UseSqlServer("con"));

// Servicio de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(op =>
{
    op.Password.RequireNonAlphanumeric = false;
    op.Password.RequiredLength = 8;
    op.Password.RequireUppercase = false;
    op.Password.RequireLowercase = false;
    op.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

// Aplicacion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();
