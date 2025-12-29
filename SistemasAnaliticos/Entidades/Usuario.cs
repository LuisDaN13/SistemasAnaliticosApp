using Microsoft.AspNetCore.Identity;
using SistemasAnaliticos.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasAnaliticos.Entidades
{
    public class Usuario : IdentityUser
    {
        // INFORMACIÓN PERSONAL BÁSICA

        [StringLength(50)]
        public string? primerNombre { get; set; } = string.Empty;

        [StringLength(50)]
        public string? segundoNombre { get; set; } = string.Empty;

        [StringLength(50)]
        public string? primerApellido { get; set; } = string.Empty;

        [StringLength(50)]
        public string? segundoApellido { get; set; } = string.Empty;

        [StringLength(15)]
        public string? noEmpleado { get; set; } = string.Empty;

        [StringLength(20)]
        public string? cedula { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? fechaNacimiento { get; set; }

        [StringLength(10)]
        public string? genero { get; set; } = string.Empty;


        // INFORMACIÓN DEMOGRÁFICA

        [StringLength(20)]
        public string? estadoCivil { get; set; } = string.Empty;

        [StringLength(5)]
        public string? tipoSangre { get; set; }

        public bool hijos { get; set; }
        public int? cantidadHijos { get; set; }


        // DIRECCIÓN 

        [StringLength(50)]
        public string? provincia { get; set; } = string.Empty;

        [StringLength(50)]
        public string? canton { get; set; } = string.Empty;

        [StringLength(50)]
        public string? distrito { get; set; } = string.Empty;

        [StringLength(255)]
        public string? direccionExacta { get; set; } = string.Empty;


        // INFORMACIÓN LABORAL

        [StringLength(100)]
        public string? profesion { get; set; } = string.Empty;

        [StringLength(100)]
        public string? puesto { get; set; } = string.Empty;

        [StringLength(100)]
        public string? departamento { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? fechaIngreso { get; set; }

        [EmailAddress]
        public string? correoEmpresa { get; set; }

        [StringLength(12)]
        public string? celularOficina { get; set; }

        public string? jefeId { get; set; }

        [StringLength(100)]
        public string? jefeNombre { get; set; }

        // Relación con el jefe
        [ForeignKey("jefeId")]
        [InverseProperty("Subordinados")]
        public virtual Usuario? Jefe { get; set; }

        // Colección de subordinados (opcional)
        [InverseProperty("Jefe")]
        public virtual ICollection<Usuario>? Subordinados { get; set; }


        [StringLength(20)]
        public string? extension { get; set; }

        [StringLength(15)]
        public string? salario { get; set; } = string.Empty;

        [StringLength(50)]
        public string? cuentaIBAN { get; set; }


        // CONTACTOS

        [StringLength(20)]
        public string? celularPersonal { get; set; }

        [EmailAddress]
        public string? correoPersonal { get; set; }

        [StringLength(20)]
        public string? telefonoHabitacion { get; set; }


        // INFORMACIÓN ADICIONAL

        [StringLength(80)]
        public string? licencias { get; set; }

        [StringLength(100)]
        public string? tipoPariente { get; set; } = string.Empty;

        [StringLength(100)]
        public string? contactoEmergencia { get; set; } = string.Empty;

        [StringLength(20)]
        public string? telefonoEmergencia { get; set; } = string.Empty;


        // ARCHIVOS Y SALUD

        // FOTOS PARA BD Y FORMULARIOA
        [Column(TypeName = "varbinary(max)")]
        public byte[]? foto { get; set; }

        [NotMapped]
        [MaxFileSize(5)]
        public IFormFile fotoFile { get; set; } = null!;

        [StringLength(500)]
        public string? padecimientosAlergias { get; set; }

        public bool estado { get; set; }


        [StringLength(100)]
        public string? sessionId { get; set; }   // Identificador único de la sesión activa

        public DateTime? lastActivityUtc { get; set; } // Última actividad en UTC


        // PROPIEDADES CALCULADAS
        public string nombreCompleto =>
            string.Join(" ", new[] { primerNombre, segundoNombre, primerApellido, segundoApellido }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        public int? edad => fechaNacimiento.HasValue ? DateTime.Now.Year - fechaNacimiento.Value.Year - (DateTime.Now.Date < fechaNacimiento.Value.AddYears(DateTime.Now.Year - fechaNacimiento.Value.Year) ? 1 : 0) : null;

        // AÑOS LABORANDO 
        public int aniosLaborando
        {
            get
            {
                var today = DateTime.Today;
                var anios = today.Year - fechaIngreso.Value.Year;

                // Ajustar si aún no ha llegado la fecha de aniversario este año
                if (fechaIngreso.Value.Date > today.AddYears(-anios))
                {
                    anios--;
                }

                return anios;
            }
        }

        // PROPIEDAD ADICIONAL: MESES LABORANDO (OPCIONAL)
        public int mesesLaborando
        {
            get
            {
                var today = DateTime.Today;
                var meses = ((today.Year - fechaIngreso.Value.Year) * 12) + today.Month - fechaIngreso.Value.Month;

                // Ajustar si el día de ingreso aún no llega este mes
                if (today.Day < fechaIngreso.Value.Day)
                {
                    meses--;
                }

                return meses;
            }
        }

        // === PROPIEDAD ADICIONAL: TIEMPO LABORAL FORMATEADO (OPCIONAL) ===
        public string tiempoLaboralFormateado
        {
            get
            {
                var anos = aniosLaborando;
                var meses = mesesLaborando % 12;

                if (anos == 0)
                {
                    return $"{meses} mes{(meses != 1 ? "es" : "")}";
                }
                else if (meses == 0)
                {
                    return $"{anos} año{(anos != 1 ? "s" : "")}";
                }
                else
                {
                    return $"{anos} año{(anos != 1 ? "s" : "")} y {meses} mes{(meses != 1 ? "es" : "")}";
                }
            }
        }
    }
}
