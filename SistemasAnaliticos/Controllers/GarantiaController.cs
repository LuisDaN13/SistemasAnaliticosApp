using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Helpers;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.Services;
using System.Runtime.InteropServices;

namespace SistemasAnaliticos.Controllers
{
    public class GarantiaController : Controller
    {
        private readonly DBContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IPermisoAlcanceService _permisoAlcanceService;
        private readonly IEmailService _emailService;

        public GarantiaController(DBContext context, UserManager<Usuario> userManager, IPermisoAlcanceService permisoAlcanceService, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _permisoAlcanceService = permisoAlcanceService;
            _emailService = emailService;
        }

        //[Authorize(Policy = "Garantia.Crear")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Garantia model)
        {
            var usuario = await _userManager.GetUserAsync(User);

            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Central America Standard Time"           // Windows
                : "America/Costa_Rica";                     // Linux/macOS

            TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);

            try
            {
                // Crear nueva garantía con los datos del formulario
                var nuevaGarantia = new Garantia
                {
                    UsuarioId = usuario.Id,
                    fechaCreacion = ahoraCR,
                    nombreEmpleado = usuario.nombreCompleto,
                    departamento = usuario.departamento,

                    // Datos del paso 1
                    moneda = model.moneda,
                    monto = model.monto,
                    aFavorDe = model.aFavorDe,
                    nombreLicitacion = model.nombreLicitacion,
                    prorroga = model.prorroga,
                    numeroGarantia = model.numeroGarantia,
                    numeroLicitacion = model.numeroLicitacion,

                    // Datos del paso 2
                    tipoLicitacion = model.tipoLicitacion,
                    fechaInicio = model.fechaInicio,
                    fechaFinalizacion = model.fechaFinalizacion,
                    plazo = model.plazo,
                    observacion = model.observacion,

                    // Estado inicial
                    estado = "Creada"
                };

                // Procesar adjuntos (hasta 5 archivos)
                var adjuntos = new[]
                {
                    model.adjuntoFile1,
                    model.adjuntoFile2,
                    model.adjuntoFile3,
                    model.adjuntoFile4,
                    model.adjuntoFile5
                };

                for (int i = 0; i < adjuntos.Length; i++)
                {
                    var archivo = adjuntos[i];
                    if (archivo != null && archivo.Length > 0)
                    {
                        // Validar tamaño (máximo 10MB) - ya validado por el atributo MaxFileSize
                        // Validar extensión
                        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                        var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

                        if (!extensionesPermitidas.Contains(extension))
                        {
                            TempData["WarningMessage"] = $"El archivo {archivo.FileName} tiene formato no permitido y no fue adjuntado.";
                            continue;
                        }

                        // Convertir archivo a byte array
                        using (var memoryStream = new MemoryStream())
                        {
                            await archivo.CopyToAsync(memoryStream);
                            var archivoBytes = memoryStream.ToArray();

                            // Asignar según el índice del adjunto
                            switch (i)
                            {
                                case 0:
                                    nuevaGarantia.datosAdjuntos1 = archivoBytes;
                                    nuevaGarantia.nombreArchivo1 = archivo.FileName;
                                    nuevaGarantia.tipoMIME1 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo1 = archivo.Length;
                                    break;
                                case 1:
                                    nuevaGarantia.datosAdjuntos2 = archivoBytes;
                                    nuevaGarantia.nombreArchivo2 = archivo.FileName;
                                    nuevaGarantia.tipoMIME2 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo2 = archivo.Length;
                                    break;
                                case 2:
                                    nuevaGarantia.datosAdjuntos3 = archivoBytes;
                                    nuevaGarantia.nombreArchivo3 = archivo.FileName;
                                    nuevaGarantia.tipoMIME3 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo3 = archivo.Length;
                                    break;
                                case 3:
                                    nuevaGarantia.datosAdjuntos4 = archivoBytes;
                                    nuevaGarantia.nombreArchivo4 = archivo.FileName;
                                    nuevaGarantia.tipoMIME4 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo4 = archivo.Length;
                                    break;
                                case 4:
                                    nuevaGarantia.datosAdjuntos5 = archivoBytes;
                                    nuevaGarantia.nombreArchivo5 = archivo.FileName;
                                    nuevaGarantia.tipoMIME5 = archivo.ContentType;
                                    nuevaGarantia.tamanoArchivo5 = archivo.Length;
                                    break;
                            }
                        }
                    }
                }

                _context.Garantia.Add(nuevaGarantia);
                await _context.SaveChangesAsync();

                try
                { 
                    // 1. Correo al empleado
                    var htmlEmpleado = PlantillasEmail.ConfirmacionEmpleadoGarantia(
                        nombreEmpleado: usuario.nombreCompleto,
                        nombreLicitacion: model.nombreLicitacion
                    );

                    await _emailService.SendEmailAsync(
                        toEmail: usuario.Email,
                        toName: usuario.nombreCompleto,
                        subject: $"Confirmación de Garantía - {usuario.nombreCompleto}",
                        htmlBody: htmlEmpleado
                    );

                    // 2. Correo de notificación a la persona de revisión (Ana Novo y Hillary Hernández)
                    if (!string.IsNullOrEmpty(usuario.jefeId))
                    {
                        var htmlRevision1 = PlantillasEmail.NotificacionRevisionGarantia(
                            nombreEmpleado: usuario.nombreCompleto,
                            nombreLicitacion: model.nombreLicitacion
                        );
                        await _emailService.SendEmailAsync(
                            toEmail: "ana.novo@sistemasanaliticos.cr",
                            toName: "Ana Novo",
                            subject: $"Solicitud de Garantía Revisión - {usuario.nombreCompleto}",
                            htmlBody: htmlRevision1
                        );

                        var htmlRevision2 = PlantillasEmail.NotificacionRevisionGarantia(
                            nombreEmpleado: usuario.nombreCompleto,
                            nombreLicitacion: model.nombreLicitacion
                        );
                        await _emailService.SendEmailAsync(
                            toEmail: "hillary.hernandez@sistemasanaliticos.cr",
                            toName: "Hillary Hernández",
                            subject: $"Solicitud de Garantía Revisión - {usuario.nombreCompleto}",
                            htmlBody: htmlRevision2
                        );
                    }
                }
                catch (Exception exEmail)
                {
                    // Solo log si hay error en correo, no interrumpir
                    Console.WriteLine($"Error enviando correo: {exEmail.Message}");
                }

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = DateOnly.FromDateTime(ahoraCR),
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Garantia",
                    Accion = "Nuevo Registro"
                };
                _context.Auditoria.Add(auditoria);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Se creó la garantía correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error en creación de garantía: {ex.Message}");
                TempData["ErrorMessage"] = "Error en la creación de la garantía.";
                return RedirectToAction("Index");
            }
        }
    }
}
