using Microsoft.AspNetCore.Authorization;

namespace SistemasAnaliticos.Auxiliares
{
    public class PermisoRequirement : IAuthorizationRequirement
    {
        public string ClavePermiso { get; }

        public PermisoRequirement(string clavePermiso)
        {
            ClavePermiso = clavePermiso;
        }
    }
}
