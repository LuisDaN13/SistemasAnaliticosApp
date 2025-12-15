using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Services
{
    public class RolPermisoService : IRolPermisoService
    {
        private readonly DBContext _context;

        public RolPermisoService(DBContext context)
        {
            _context = context;
        }

        public async Task AsignarPermisosARolAsync(string rolId, List<string> permisos)
        {
            var existentes = _context.RolPermisos
                .Where(rp => rp.RolId == rolId);

            _context.RolPermisos.RemoveRange(existentes);

            foreach (var permiso in permisos)
            {
                _context.RolPermisos.Add(new RolPermiso
                {
                    RolId = rolId,
                    Clave = permiso
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
