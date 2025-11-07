using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Models.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class PermisoController : Controller
    {
        private readonly DBContext _context;


        public PermisoController(DBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Permiso model)
        {
            // Detectar sistema operativo y usar el ID de zona horaria adecuado
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
            DateOnly hoy = DateOnly.FromDateTime(ahoraCR);

            try
            {
                var fotoService = new CodigoFotos();
                var adjuntoService = new ProcesarAdjuntos();

                var nuevo = new Permiso
                {
                    fechaIngreso = ahoraCR,
                    nombreEmpleado = "Luis",
                    tipo = model.tipo,

                    fechaInicio = model.fechaInicio,
                    fechaFinalizacion = model.fechaFinalizacion,
                    fechaRegresoLaboral = model.fechaRegresoLaboral,
                    horaCita = model.horaCita,

                    motivo = model.motivo,
                    comentarios = model.comentarios,

                    foto = fotoService.ConvertFileToByteArrayAsync(model.fotoFile).Result,

                    datosAdjuntos = await adjuntoService.ProcesarArchivoAdjunto(model.adjuntoFile),
                    nombreArchivo = model.adjuntoFile?.FileName,
                    tipoMIME = model.adjuntoFile?.ContentType,
                    tamanoArchivo = model.adjuntoFile?.Length,

                    estado = "Creada"
                };

                _context.Permiso.Add(nuevo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Usuario", "Index");
            }
        }

        public async Task<List<Permiso>> TraerIncapacidades()
        {
            var incapacidades = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.tipo == "Incapacidad" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Select(x => new Permiso
                {
                    idPermiso = x.idPermiso,
                    nombreEmpleado = x.nombreEmpleado,
                    fechaIngreso = x.fechaIngreso,
                    fechaInicio = x.fechaInicio,
                    fechaFinalizacion = x.fechaFinalizacion,
                    fechaRegresoLaboral = x.fechaRegresoLaboral,
                    motivo = x.motivo,
                    comentarios = x.comentarios,
                    foto = x.foto,
                    estado = x.estado
                })
                .ToListAsync();

            return incapacidades;
        }
    }
}