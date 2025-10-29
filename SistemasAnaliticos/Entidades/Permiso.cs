using Humanizer;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.SqlServer.Server;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SistemasAnaliticos.Entidades
{
    public class Permiso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long idPermiso { get; set; }

        [Required]
        public DateTime fechaIngreso { get; set; }

        [Required]
        public string nombreEmpleado { get; set; }

        [Required]
        public string tipo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? fechaInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? fechaFinalizacion { get; set; }

        [DataType(DataType.Date)]
        public DateTime? fechaRegresoLaboral { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? horaCita { get; set; }
        public string? motivo { get; set; }
        public string? comentarios { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[]? foto { get; set; }

        [NotMapped]
        [MaxFileSize(5)]
        public IFormFile? fotoFile { get; set; } = null!;

        [NotMapped]
        [MaxFileSize(10)]
        public IFormFile? adjuntoFile { get; set; } = null!;

        // Datos adjuntos para SQL Server
        [Column(TypeName = "varbinary(max)")]
        public byte[]? datosAdjuntos { get; set; }
        public string? nombreArchivo { get; set; }
        public string? tipoMIME { get; set; }
        public long? tamanoArchivo { get; set; }

        [Required]
        public string estado { get; set; }
    }
}