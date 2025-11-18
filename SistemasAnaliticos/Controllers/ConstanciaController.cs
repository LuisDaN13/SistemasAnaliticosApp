using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using SistemasAnaliticos.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Models.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class ConstanciaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IConstanciaService _constanciaService;


        public ConstanciaController(DBContext context, UserManager<Usuario> userManager, IConstanciaService constanciaService)
        {
            _context = context;
            _userManager = userManager;
            _constanciaService = constanciaService;
        }

        public async Task<IActionResult> VerConstancias(int page = 1)
        {
            int pageSize = 3;

            var totalConstancias = await _context.Constancia
                .Where(p => p.estado == "Creada")
                .CountAsync();

            var constancias = await _context.Constancia
                .AsNoTracking()
                .Where(p => p.estado == "Creada")
                .OrderByDescending(x => x.fechaPedido)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ConstanciaDTO
                {
                    idConstancia = p.idConstancia,
                    fechaPedido = p.fechaPedido,
                    nombreEmpleado = p.nombrePersona,
                    tipo = p.tipo,
                    dirijido = p.dirijido,
                    fechaRequerida = p.fechaRequerida,
                    comentarios = p.Comentarios,
                    nombreArchivo = p.nombreArchivo,
                    tipoMIME = p.tipoMIME,
                    tamanoArchivo = p.tamanoArchivo,
                    estado = p.estado
                })
                .ToListAsync();

            var viewModel = new PaginacionConstanciasDTO
            {
                Constancias = constancias,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalConstancias / (double)pageSize)
            };

            return View(viewModel);
        }

        // ENDPOINT PARA TENER TODAS LAS CONSTANCIAS (sin paginación)
        public async Task<IActionResult> ObtenerTodasLasConstancias()
        {
            try
            {
                var todosLasConstancias = await _context.Constancia
                    .AsNoTracking()
                    .OrderByDescending(x => x.fechaPedido)
                    .Select(p => new {
                        p.idConstancia,
                        p.fechaPedido,
                        p.nombrePersona,
                        p.tipo,
                        p.dirijido,
                        p.fechaRequerida,
                        p.Comentarios,
                        p.nombreArchivo,
                        p.tipoMIME,
                        p.tamanoArchivo,
                        p.estado,
                    })
                    .ToListAsync();

                return Ok(todosLasConstancias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        //METODO SEGUIDO DE LA PAGINACION PARA FILTROS JUNTOS
        public async Task<IActionResult> ObtenerConstanciasFiltrados([FromQuery] string[] tipos, [FromQuery] string[] estados)
        {
            var query = _context.Constancia.AsNoTracking().AsQueryable();

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
                .OrderByDescending(x => x.fechaPedido )
                .Select(p => new {
                    p.idConstancia,
                    p.fechaPedido,
                    p.nombrePersona,
                    p.tipo,
                    p.dirijido,
                    p.fechaRequerida,
                    p.Comentarios,
                    p.nombreArchivo,
                    p.tipoMIME,
                    p.tamanoArchivo,
                    p.estado,
                })
                .ToListAsync();

            return Ok(permisos);
        }

        // ENDPOINT PARA OBTENER CONTADORES DE FILTROS
        public async Task<IActionResult> ObtenerContadoresFiltros()
        {
            try
            {
                var contadores = await _context.Constancia
                    .AsNoTracking()
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        Laboral = g.Count(p => p.tipo == "Laboral"),
                        Salarial = g.Count(p => p.tipo == "Salarial"),
                        Aprobado = g.Count(p => p.estado == "Aprobada"),
                        Pendiente = g.Count(p => p.estado == "Pendiente"),
                        Rechazado = g.Count(p => p.estado == "Rechazada")
                    })
                    .FirstOrDefaultAsync();

                return Ok(contadores ?? new
                {
                    Laboral = 0,
                    Salarial = 0,
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazado = 0
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Laboral = 0,
                    Salarial = 0,
                    Aprobado = 0,
                    Pendiente = 0,
                    Rechazado = 0
                });
            }
        }


        [HttpGet]
        [Route("Constancia/descargar-adjunto/{id}")]
        public async Task<IActionResult> DescargarAdjunto(long id)
        {
            var constancia = await _context.Constancia
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.idConstancia == id);

            if (constancia?.datosAdjuntos == null || constancia.datosAdjuntos.Length == 0)
            {
                return NotFound("No se encontró el archivo adjunto");
            }

            // Retornar el archivo
            return File(
                constancia.datosAdjuntos,
                constancia.tipoMIME ?? "application/octet-stream",
                constancia.nombreArchivo ?? "archivo_adjunto"
            );
        }

        public async Task<IActionResult> Create(Constancia model)
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

                var user = await _userManager.GetUserAsync(User);

                if (model.tipo == "Laboral")
                {
                    var pdfBytes = await _constanciaService.GenerarConstanciaLaboral(user.nombreCompleto, user.cedula, user.departamento, user.fechaIngreso, user.puesto, model.dirijido);

                    var nuevo = new Constancia
                    {
                        fechaPedido = ahoraCR,
                        nombrePersona = user.nombreCompleto,
                        tipo = model.tipo,

                        dirijido = model.dirijido,
                        fechaRequerida = model.fechaRequerida,
                        Comentarios = model.Comentarios,

                        datosAdjuntos = pdfBytes,
                        nombreArchivo = $"Constancia_Laboral_{user.primerNombre}{user.primerApellido}.pdf",
                        tipoMIME = "application/pdf",
                        tamanoArchivo = pdfBytes.Length,

                        estado = "Creada"
                    };

                    _context.Constancia.Add(nuevo);
                    await _context.SaveChangesAsync();
                } else
                {
                    var nuevo = new Constancia
                    {
                        fechaPedido = ahoraCR,
                        nombrePersona = user.nombreCompleto,
                        tipo = model.tipo,

                        dirijido = model.dirijido,
                        fechaRequerida = model.fechaRequerida,
                        Comentarios = model.Comentarios,

                        estado = "Creada"
                    };

                    _context.Constancia.Add(nuevo);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Se creó la constancia correctamente.";
                return RedirectToAction("Index", "Permiso");
            }
            catch
            {
                TempData["ErrorMessage"] = "Error en la creación de la constancia.";
                return RedirectToAction("Index", "Permiso");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HacerConstanciaSalarial(string id, int salarioBruto, int deducciones, int salarioNeto)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de empleado válido.";
                return RedirectToAction("Index", "Usuario");
            }

            var constancia = await _context.Constancia.FindAsync(id);
            if (constancia == null)
            {
                TempData["ErrorMessage"] = "El empleado que intentas editar no existe o fue eliminado.";
                return RedirectToAction("Index", "Usuario");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var pdfBytes = await _constanciaService.GenerarConstanciaSalarial(user.nombreCompleto, user.cedula, user.departamento, user.fechaIngreso, user.puesto, salarioBruto, deducciones, salarioNeto);

                // 🔹 Actualizar campos personales
                constancia.datosAdjuntos = pdfBytes;
                constancia.nombreArchivo = $"Constancia_Salarial_{user.primerNombre}{user.primerApellido}.pdf",;
                constancia.tipoMIME = "application/pdf";
                constancia.tamanoArchivo = pdfBytes.Length;

                // 🔹 Guardar cambios
                _context.Update(constancia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Se cre[o correctamente la constancia salarial.";
                return RedirectToAction("VerConstancias", "Constancia");
            }
            catch (Exception ex)
            {
                // 🧩 Manejo de error
                TempData["ErrorMessage"] = "Ocurrió un error al crear la constancia: " + ex.Message;
                return RedirectToAction("VerConstancias", "Constancia");
            }
        }
    }
}