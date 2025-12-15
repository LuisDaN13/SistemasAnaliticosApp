using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemasAnaliticos.Entidades
{
    public class Rol : IdentityRole
    {
        [Required]
        public bool estado { get; set; }
    }
}
