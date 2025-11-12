using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> VerPermisos(int page = 1)
        {
            int pageSize = 4; // 👈 cantidad fija de registros por página

            var totalPermisos = await _context.Permiso
                .Where(p => p.estado == "Creada")
                .CountAsync();

            var permisos = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new PaginacionPermisosDTO
            {
                Permisos = permisos,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalPermisos / (double)pageSize)
            };

            return View(viewModel);
        }

        //METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerTodosLosPermisos()
        {
            var todosLosPermisos = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.estado == "Creada") // Mantener el mismo filtro que en VerPermisos
                .OrderByDescending(x => x.fechaIngreso)
                .ToListAsync();

            return Json(todosLosPermisos);
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
                TempData["SuccessMessage"] = "Se creó el permiso correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error en la creación del permiso.";
                return RedirectToAction("Usuario", "Index");
            }
        }
    }
}