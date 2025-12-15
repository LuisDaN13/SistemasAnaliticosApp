using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class RolPermiso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK a AspNetRoles
        public string RolId { get; set; }

        // Clave tipo: Modulo.Accion
        public string Clave { get; set; }
    }
}
