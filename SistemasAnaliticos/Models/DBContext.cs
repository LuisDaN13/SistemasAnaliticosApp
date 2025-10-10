using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;

namespace SistemasAnaliticos.Models
{
    public class DBContext : IdentityDbContext<Usuario, Rol, String>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // LLAMAR PRIMERO AL MÉTODO BASE
            base.OnModelCreating(builder);

            // LUEGO TU CONFIGURACIÓN PERSONALIZADA
            builder.Entity<Usuario>(entity =>
            {
                entity.Ignore(u => u.Email);
                entity.Ignore(u => u.NormalizedEmail);
                entity.Ignore(u => u.EmailConfirmed);
                entity.Ignore(u => u.SecurityStamp);
                entity.Ignore(u => u.ConcurrencyStamp);
                entity.Ignore(u => u.PhoneNumber);
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.TwoFactorEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.AccessFailedCount);
            });

            builder.Entity<Rol>(entity =>
            {
                entity.Ignore(u => u.ConcurrencyStamp);
            });
        }
    }
}
