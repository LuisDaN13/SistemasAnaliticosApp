using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Models
{
    public class DBContext : IdentityDbContext<Usuario, Rol, String>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<UsuarioSesion> UsuarioSesion { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<Constancia> Constancia { get; set; }
        public DbSet<Beneficio> Beneficio { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // LLAMAR PRIMERO AL MÉTODO BASE
            base.OnModelCreating(builder);

            // LUEGO TU CONFIGURACIÓN PERSONALIZADA
            builder.Entity<UsuarioSesion>(entity =>
            {
                entity.ToTable("UsuarioSesion"); // Nombre de tabla
                entity.HasKey(us => us.Id); // Clave primaria

                entity.Property(us => us.SessionId)
                      .IsRequired()
                      .HasMaxLength(128);

                entity.Property(us => us.LoginDate)
                      .IsRequired();

                entity.Property(us => us.IsActive)
                      .IsRequired();

                // Configurar la relación FOREIGN KEY explícitamente
                entity.HasOne(us => us.User)
                      .WithMany() // Si no tienes colección en Usuario
                      .HasForeignKey(us => us.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}