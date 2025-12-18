using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SistemasAnaliticos.Entidades;
using System.Reflection.Emit;

namespace SistemasAnaliticos.Models
{
    public class DBContext : IdentityDbContext<Usuario, Rol, String>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        // TABLAS DE BASE DE DATOS
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<UsuarioSesion> UsuarioSesion { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<Constancia> Constancia { get; set; }
        public DbSet<Beneficio> Beneficio { get; set; }
        public DbSet<Fotos> Fotos { get; set; }
        public DbSet<Noticias> Noticias { get; set; }
        public DbSet<LiquidacionViatico> LiquidacionViatico { get; set; }
        public DbSet<LiquidacionViaticoDetalle> LiquidacionViaticoDetalle { get; set; }
        public DbSet<AlcanceUsuario> AlcanceUsuario { get; set; }

        // REGLAS DE MODELO
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // LLAMAR PRIMERO AL MÉTODO BASE
            base.OnModelCreating(builder);

            builder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.primerNombre);
                entity.HasIndex(u => u.noEmpleado).IsUnique();
                entity.HasIndex(u => u.cedula).IsUnique();
                entity.HasIndex(u => u.departamento);       
                entity.HasIndex(u => u.puesto);         
                entity.HasIndex(u => u.correoEmpresa).IsUnique();
                entity.HasIndex(u => u.estado);         
                entity.HasIndex(u => u.fechaIngreso);    
            });

            builder.Entity<UsuarioSesion>(entity =>
            {
                entity.ToTable("UsuarioSesion"); 
                entity.HasKey(us => us.Id);

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

            builder.Entity<LiquidacionViatico>().HasMany(l => l.Detalles).WithOne(d => d.Liquidacion).HasForeignKey(d => d.idViatico).OnDelete(DeleteBehavior.Cascade); // si se borra la liquidación, se borran los detalles

            builder.Entity<RolPermiso>(entity =>
            {
                entity.HasKey(rp => rp.Id);

                entity.Property(rp => rp.Clave)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(rp => rp.RolId)
                      .IsRequired();

                entity.HasIndex(rp => new { rp.RolId, rp.Clave })
                      .IsUnique();
            });

            builder.Entity<AlcanceUsuario>(entity =>
            {
                entity.HasKey(x => x.idAlcance);
                entity.Property(x => x.alcance).IsRequired().HasMaxLength(50);
                entity.HasIndex(x => x.rolId).IsUnique();
            });
        }
    }
}