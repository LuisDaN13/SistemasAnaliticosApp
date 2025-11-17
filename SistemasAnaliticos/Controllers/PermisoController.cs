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
            var sw = Stopwatch.StartNew();

            int pageSize = 3;

            var totalPermisos = await _context.Permiso
                .Where(p => p.estado == "Creada")
                .CountAsync();

            var permisos = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PermisoDTO
                {
                    idPermiso = p.idPermiso,
                    fechaIngreso = p.fechaIngreso,
                    nombreEmpleado = p.nombreEmpleado,
                    tipo = p.tipo,
                    fechaInicio = p.fechaInicio,
                    fechaFinalizacion = p.fechaFinalizacion,
                    fechaRegresoLaboral = p.fechaRegresoLaboral,
                    motivo = p.motivo,
                    comentarios = p.comentarios,
                    foto = p.foto,
                    nombreArchivo = p.nombreArchivo,
                    tipoMIME = p.tipoMIME,
                    tamanoArchivo = p.tamanoArchivo,
                    estado = p.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionPermisosDTO
            {
                Permisos = permisos,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalPermisos / (double)pageSize)
            };

            sw.Stop();
            Debug.WriteLine($"⏱ Consulta tardó: {sw.ElapsedMilliseconds} ms");
            return View(viewModel);
        }

        // ENDPOINT PARA TENER TODOS LO PERMISOS (sin paginación)
        public async Task<IActionResult> ObtenerTodosLosPermisos()
        {
            try
            {
                var todosLosPermisos = await _context.Permiso
                    .AsNoTracking()
                    .OrderByDescending(x => x.fechaIngreso)
                    .Select(p => new {
                        p.idPermiso,
                        p.nombreEmpleado,
                        p.tipo,
                        p.estado,
                        p.fechaIngreso,
                        p.fechaInicio,
                        p.fechaFinalizacion,
                        p.fechaRegresoLaboral,
                        p.motivo,
                        p.comentarios,
                        p.foto,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo
                    })
                    .ToListAsync();

                return Ok(todosLosPermisos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        //METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerPermisosFiltrados([FromQuery] string[] tipos, [FromQuery] string[] estados)
        {
            var query = _context.Permiso.AsNoTracking().AsQueryable();

            // Aplicar filtros
            if (tipos != null && tipos.Length > 0)
            {
                query = query.Where(p => tipos.Contains(p.tipo));
            }

            if (estados != null && estados.Length > 0)
            {
                var estadosMapeados = estados.Select(e =>
                    e == "Aprobado" ? "Creada" :
                    e == "Pendiente" ? "Pendiente" :
                    "Rechazada").ToArray();

                query = query.Where(p => estadosMapeados.Contains(p.estado));
            }

            var permisos = await query
                .OrderByDescending(x => x.fechaIngreso)
                .Select(p => new {
                    p.idPermiso,
                    p.nombreEmpleado,
                    p.tipo,
                    p.estado,
                    p.fechaIngreso,
                    p.fechaInicio,
                    p.fechaFinalizacion,
                    p.fechaRegresoLaboral,
                    p.motivo,
                    p.comentarios,
                    p.foto,
                    p.nombreArchivo,
                    p.tipoMIME,
                    p.tamanoArchivo
                })
                .ToListAsync();

            return Ok(permisos);
        }

        // ENDPOINT PARA OBTENER CONTADORES DE FILTROS
        public async Task<IActionResult> ObtenerContadoresFiltros()
        {
            try
            {
                var contadores = await _context.Permiso
                    .AsNoTracking()
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        CitaMedica = g.Count(p => p.tipo == "Cita Médica"),
                        Vacaciones = g.Count(p => p.tipo == "Vacaciones"),
                        Incapacidad = g.Count(p => p.tipo == "Incapacidad"),
                        Teletrabajo = g.Count(p => p.tipo == "Teletrabajo"),
                        Especial = g.Count(p => p.tipo == "Especial"),
                        Aprobado = g.Count(p => p.estado == "Creada" || p.estado == "Aprobada"),
                        Pendiente = g.Count(p => p.estado == "Pendiente"),
                        Rechazado = g.Count(p => p.estado == "Rechazada")
                    })
                    .FirstOrDefaultAsync();

                return Ok(contadores ?? new
                {
                    CitaMedica = 0,
                    Vacaciones = 0,
                    Incapacidad = 0,
                    Teletrabajo = 0,
                    Especial = 0,
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazado = 0
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    CitaMedica = 0,
                    Vacaciones = 0,
                    Incapacidad = 0,
                    Teletrabajo = 0,
                    Especial = 0,
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazado = 0
                });
            }
        }

        [HttpGet]
        [Route("Permiso/Details/{id}")]
        public async Task<ActionResult> Details(long id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de permiso válido.";
                return RedirectToAction("VerPermisos", "Permiso");
            }

            var permiso = await _context.Permiso.FindAsync(id);

            if (permiso == null)
            {
                TempData["ErrorMessage"] = "El permiso que intentas editar no existe o fue eliminado.";
                return RedirectToAction("VerPermisos", "Permiso");
            }

            return View(permiso);
        }


        [HttpGet]
        [Route("Permiso/descargar-adjunto/{id}")]
        public async Task<IActionResult> DescargarAdjunto(long id)
        {
            var permiso = await _context.Permiso
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.idPermiso == id);

            if (permiso?.datosAdjuntos == null || permiso.datosAdjuntos.Length == 0)
            {
                return NotFound("No se encontró el archivo adjunto");
            }

            // Retornar el archivo
            return File(
                permiso.datosAdjuntos,
                permiso.tipoMIME ?? "application/octet-stream",
                permiso.nombreArchivo ?? "archivo_adjunto"
            );
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
                return RedirectToAction(nameof(Index));
            }
        }
    }
}