using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using SistemasAnaliticos.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using static SistemasAnaliticos.Auxiliares.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly SignInManager<Usuario> signInManager;
        private readonly UserManager<Usuario> userManager;
        private readonly RoleManager<Rol> roleManager;
        private readonly DBContext _context;
        private readonly IMemoryCache _cache;

        public UsuarioController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager, RoleManager<Rol> roleManager, DBContext context, IMemoryCache cache)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _context = context;
            _cache = cache;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // GETS DE DROPDOWNS Y LISTAS

        [HttpGet]
        public IActionResult GetTecnicos()
        {
            try
            {
                var tecnicos = userManager.Users
                    .Where(u => u.departamento == "Técnicos NCR")
                    .OrderBy(u => u.primerNombre)
                    .ThenBy(u => u.primerApellido)
                    .Select(u => new
                    {
                        id = u.Id,
                        nombre = u.primerNombre + " " + u.primerApellido
                    })
                    .ToList();

                return Json(new { success = true, tecnicos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJefes()
        {
            try
            {
                var usuarioEnRol = await userManager.GetUsersInRoleAsync("Jefatura");

                var jefes = usuarioEnRol
                    .Where(u => u.estado == true)
                    .OrderBy(u => u.primerNombre)
                    .ThenBy(u => u.primerApellido)
                    .Select(u => new
                    {
                        Id = u.Id,
                        NombreCompleto = $"{u.nombreCompleto}",
                        Departamento = u.departamento
                    })
                    .ToList();

                return Json(new { success = true, jefes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Nombre = r.Name
                    })
                    .ToListAsync();

                return Json(new { success = true, roles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // SESSION MANAGEMENT

        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Usuario");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View(model);
            }

            if (!user.estado)
            {
                ModelState.AddModelError("", "Su cuenta está desactivada. Contacte al administrador.");
                return View(model);
            }

            var result = await signInManager.CheckPasswordSignInAsync(
                user, model.Password, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View(model);
            }

            // 🔍 Detectar si es el primer inicio de sesión
            var esPrimerInicio = user.lastActivityUtc == null;

            // 🔄 Generar nueva sesión (invalida las anteriores)
            var newSessionId = Guid.NewGuid().ToString();
            user.sessionId = newSessionId;
            user.lastActivityUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            // --- NUEVO: invalidar cache del usuario para que el middleware lea la BD actualizada ---
            try
            {
                var cacheKey = $"UserSession_{user.Id}";
                _cache.Remove(cacheKey);
                _cache.Remove($"LastActivity_{user.Id}");
                _cache.Remove($"DbUpdate_{user.Id}");
            }
            catch
            {
                // No bloquear el login por problemas de cache
            }

            // 🔑 Crear principal con claim SessionId
            var principal = await signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            var existing = identity.FindFirst("SessionId");
            if (existing != null)
                identity.RemoveClaim(existing);

            identity.AddClaim(new Claim("SessionId", newSessionId));

            // 🍪 Firmar cookie de Identity
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                });

            // 📋 Si es primer inicio, almacenar info para mostrar modal
            if (esPrimerInicio)
            {
                TempData["ShowPasswordChangeModal"] = "true";
                TempData["FirstLoginUserId"] = user.Id;
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }




        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX = PRESENTAR A LOS EMPLEADOS CON CARDS PARA SCINICIO DE SESIÓN DE LA APLICACIÓN CON CORREO Y CONTRASEÑA
        [Authorize(Policy = "Usuarios.Ver")]
        public async Task<ActionResult> Index()
        {
            var cards = await _context.Users
                .AsNoTracking()
                .OrderByDescending(x => x.primerNombre)
                .Select(x => new CardsViewModel
                {
                    Id = x.Id,
                    Nombre = x.primerNombre + " " + x.primerApellido + " " + x.segundoApellido,
                    Puesto = x.puesto,
                    Departamento = x.departamento,
                    CorreoEmp = x.correoEmpresa,
                    TelefonoEmp = x.celularOficina,
                    CorreoPerso = x.correoPersonal,
                    TelefonoPerso = x.celularPersonal,
                    Estado = x.estado,
                    Foto = x.foto
                })
                .ToListAsync();
            return View(cards);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DETAILS = VER MÁS INFORMACIÓN DEL EMPLEADO
        [Authorize(Policy = "Usuarios.Detalles")]
        public async Task<ActionResult> Details(string id)
        {
            var details = await _context.Users
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            int diasAcumulados = details.AcumularDiasHastaHoy();

            // 3. ✅ GUARDAR EN BD SI HUBO CAMBIOS
            if (diasAcumulados > 0)
            {
                // UserManager guarda los cambios automáticamente
                await userManager.UpdateAsync(details);
            }
            
            return View(details);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CREATE = REGISTRAR EMPLEADO CON FORMULARIO Y TODO
        [Authorize(Policy = "Usuarios.Crear")]
        [HttpPost]
        public async Task<ActionResult> Create(Usuario model, string rolSeleccionado)
        {
            try
            {
                string email = model.correoEmpresa ?? "sinCorreoAun@mailcom";
                string userName = model.correoEmpresa ?? "sinCorreoAun@mailcom";
                var fotoService = new CodigoFotos();

                var nuevo = new Usuario
                {
                    primerNombre = model.primerNombre,
                    segundoNombre = model.segundoNombre,
                    primerApellido = model.primerApellido,
                    segundoApellido = model.segundoApellido,

                    noEmpleado = model.noEmpleado,
                    cedula = model.cedula.Replace("-", ""),
                    fechaNacimiento = model.fechaNacimiento,
                    genero = model.genero,
                    estadoCivil = model.estadoCivil,
                    tipoSangre = model.tipoSangre,

                    hijos = model.hijos,
                    cantidadHijos = model.cantidadHijos,

                    provincia = model.provincia,
                    canton = model.canton,
                    distrito = model.distrito,
                    direccionExacta = model.direccionExacta,

                    profesion = model.profesion,
                    puesto = model.puesto,
                    departamento = model.departamento,
                    fechaIngreso = model.fechaIngreso,
                    correoEmpresa = model.correoEmpresa,

                    Email = email,
                    UserName = userName,

                    celularOficina = model.celularOficina?.Replace(" ", ""),

                    jefeId = model.jefeId,
                    jefeNombre = model.jefeNombre,

                    extension = model.extension,
                    salario = model.salario?.Replace(" ", ""),
                    cuentaIBAN = model.cuentaIBAN,
                    celularPersonal = model.celularPersonal?.Replace(" ", ""),
                    correoPersonal = model.correoPersonal,
                    telefonoHabitacion = model.telefonoHabitacion?.Replace(" ", ""),

                    licencias = model.licencias,
                    tipoPariente = model.tipoPariente,
                    contactoEmergencia = model.contactoEmergencia,
                    telefonoEmergencia = model.telefonoEmergencia?.Replace(" ", ""),
                    padecimientosAlergias = model.padecimientosAlergias,
                    estado = true,

                    foto = fotoService.ConvertFileToByteArrayAsync(model.fotoFile).Result
                };

                var pass = "123456789";

                var resultado = await userManager.CreateAsync(nuevo, pass);
                if (resultado.Succeeded)
                {
                    // Agregar roles seleccionados
                    if (!string.IsNullOrEmpty(rolSeleccionado))
                    {
                        await userManager.AddToRoleAsync(nuevo, rolSeleccionado);
                    }
                    else
                    {
                        // Si no se seleccionó ningún rol, asignar "Empleado Normal" por defecto
                        await userManager.AddToRoleAsync(nuevo, "Empleado Normal");
                    }

                    // Detectar sistema operativo y usar el ID de zona horaria adecuado
                    string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "Central America Standard Time"           // Windows
                        : "America/Costa_Rica";                     // Linux/macOS

                    TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                    DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                    var usuario = await userManager.GetUserAsync(User);

                    // Auditoría
                    var auditoria = new Auditoria
                    {
                        Fecha = hoy,
                        Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                        Usuario = usuario.nombreCompleto ?? "Desconocido",
                        Tabla = "Usuario",
                        Accion = "Nuevo registro"
                    };
                    _context.Auditoria.Add(auditoria);

                    TempData["SuccessMessage"] = "El empleado se creó correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var duplicateUserError = resultado.Errors
                        .FirstOrDefault(e => e.Code == "DuplicateUserName");

                    if (duplicateUserError != null)
                    {
                        TempData["ErrorMessage"] = "Ya existe un usuario registrado con ese correo electrónico.";
                        return RedirectToAction("Index", "Usuario");
                    }
                    else
                    {
                        // Manejar otros errores
                        TempData["ErrorMessage"] = "Error en la creación del empleado.";
                        return RedirectToAction("Index", "Usuario");
                    }
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Capturar excepciones de base de datos
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    // Verificar si es error de cédula duplicada
                    if (sqlEx.Message.Contains("IX_AspNetUsers_cedula"))
                    {
                        TempData["ErrorMessage"] = "Ya existe un usuario registrado con la misma cédula, intente de nuevo!";
                        return RedirectToAction("Index", "Usuario");
                    }
                }

                // Si no es ninguno de los errores conocidos, mostrar mensaje genérico
                TempData["ErrorMessage"] = $"Error en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return RedirectToAction("Index", "Usuario");
            }
            catch
            {
                TempData["ErrorMessage"] = "Error en el proceso.";
                return RedirectToAction("Index", "Usuario");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // EDIT = MODIFICAR INFORMACIÓN DEL EMPLEADO
        [Authorize(Policy = "Usuarios.Editar")]
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de usuario válido.";
                return RedirectToAction("Index", "Usuario");
            }

            var usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "El usuario que intentas editar no existe o fue eliminado.";
                return RedirectToAction("Index", "Usuario");
            }

            return View(usuario);
        }

        [Authorize(Policy = "Usuarios.Editar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Usuario model)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No se proporcionó un identificador de empleado válido.";
                return RedirectToAction("Index", "Usuario");
            }

            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "El empleado que intentas editar no existe o fue eliminado.";
                return RedirectToAction("Index", "Usuario");
            }

            try
            {
                var fotoService = new CodigoFotos();

                // 🔹 Actualizar campos personales
                usuario.primerNombre = model.primerNombre;
                usuario.segundoNombre = model.segundoNombre;
                usuario.primerApellido = model.primerApellido;
                usuario.segundoApellido = model.segundoApellido;

                usuario.noEmpleado = model.noEmpleado;
                usuario.cedula = model.cedula?.Replace("-", "");
                usuario.fechaNacimiento = model.fechaNacimiento;
                usuario.genero = model.genero;
                usuario.estadoCivil = model.estadoCivil;
                usuario.tipoSangre = model.tipoSangre;

                usuario.hijos = model.hijos;
                usuario.cantidadHijos = model.cantidadHijos;

                // 🔹 Dirección
                usuario.provincia = model.provincia;
                usuario.canton = model.canton;
                usuario.distrito = model.distrito;
                usuario.direccionExacta = model.direccionExacta;

                // 🔹 Laboral
                usuario.profesion = model.profesion;
                usuario.puesto = model.puesto;
                usuario.departamento = model.departamento;
                usuario.fechaIngreso = model.fechaIngreso;
                usuario.correoEmpresa = model.correoEmpresa;

                // 🔹 Credenciales (opcionalmente se pueden actualizar)
                usuario.Email = model.correoEmpresa;
                usuario.UserName = model.correoEmpresa;

                usuario.celularOficina = model.celularOficina?.Replace(" ", "");

                if (!string.IsNullOrEmpty(model.jefeId))
                {
                    usuario.jefeId = model.jefeId;
                    var busquedaJefe = await _context.Users.FindAsync(model.jefeId);

                    if (busquedaJefe != null)
                    {
                        usuario.jefeNombre = busquedaJefe.nombreCompleto;
                    }
                    else
                    {
                        usuario.jefeNombre = null;
                    }
                }
                else
                {
                    usuario.jefeId = null;
                    usuario.jefeNombre = null;
                }

                usuario.extension = model.extension;
                usuario.salario = model.salario?.Replace(" ", "");
                usuario.cuentaIBAN = model.cuentaIBAN;
                usuario.celularPersonal = model.celularPersonal?.Replace(" ", "");
                usuario.correoPersonal = model.correoPersonal;
                usuario.telefonoHabitacion = model.telefonoHabitacion?.Replace(" ", "");

                usuario.licencias = model.licencias;
                usuario.tipoPariente = model.tipoPariente;
                usuario.contactoEmergencia = model.contactoEmergencia;
                usuario.telefonoEmergencia = model.telefonoEmergencia?.Replace(" ", "");
                usuario.padecimientosAlergias = model.padecimientosAlergias;

                // 🔹 Foto (solo si subió una nueva)
                if (model.fotoFile != null)
                {
                    usuario.foto = await fotoService.ConvertFileToByteArrayAsync(model.fotoFile);
                }

                // 🔹 Guardar cambios
                _context.Update(usuario);


                await _context.SaveChangesAsync();

                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var user = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = user.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Edición del registro, del empleado" + usuario.primerNombre + " " + usuario.primerApellido
                };
                _context.Auditoria.Add(auditoria);

                TempData["SuccessMessage"] = "El empleado se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                // Capturar excepciones de base de datos
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    // Verificar si es error de cédula duplicada
                    if (sqlEx.Message.Contains("IX_AspNetUsers_cedula"))
                    {
                        TempData["ErrorMessage"] = "Ya existe un usuario registrado con la misma cédula, intente de nuevo!";
                        return RedirectToAction("Index", "Usuario");
                    }
                }

                // Si no es ninguno de los errores conocidos, mostrar mensaje genérico
                TempData["ErrorMessage"] = $"Error en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return RedirectToAction("Index", "Usuario");
            }
            catch (Exception ex)
            {
                // 🧩 Manejo de error
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar el empleado: " + ex.Message;
                return RedirectToAction("Index", "Usuario");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INACTIVAR = CAMBIO DE ESTADO DEL EMPLEADO (ACTIVO / INACTIVO)
        [Authorize(Policy = "Usuarios.Inactivar")]
        [HttpPost("Usuario/Inactivar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inactivar(string id)
        {
            try
            {
                var usuario = await _context.Users.FindAsync(id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                usuario.estado = !usuario.estado;

                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var user = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = user.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Cambio de Estado de " + usuario.primerNombre + " " + usuario.primerApellido
                };
                _context.Auditoria.Add(auditoria);

                await _context.SaveChangesAsync();

                return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CAMBIAR CONTRASEÑA = PERMITIR AL ADMINISTRADOR CAMBIAR LA CONTRASEÑA DE OTRO USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var user = await userManager.FindByIdAsync(model.Id.ToString());

            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.NuevaContrasena);

            if (!result.Succeeded)
            {
                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Cambió de Contraseña de " + user.primerNombre + " " + user.primerApellido
                };
                _context.Auditoria.Add(auditoria);

                return Json(new
                {
                    success = false,
                    message = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }

            // 🔥 INVALIDAR TODAS LAS SESIONES
            await userManager.UpdateSecurityStampAsync(user);

            // --- NUEVO: limpiar cache del usuario al invalidar sesiones ---
            try
            {
                _cache.Remove($"UserSession_{user.Id}");
                _cache.Remove($"LastActivity_{user.Id}");
                _cache.Remove($"DbUpdate_{user.Id}");
            }
            catch
            {
            }

            // 🔐 Logout inmediato si es el mismo usuario
            var usuarioActualId = userManager.GetUserId(User);

            if (usuarioActualId == user.Id)
            {
                await signInManager.SignOutAsync();

                return Json(new
                {
                    success = true,
                    message = "Contraseña cambiada. Debe iniciar sesión nuevamente.",
                    logout = true
                });
            }

            return Json(new
            {
                success = true,
                message = "Contraseña cambiada correctamente"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena2(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            // Generar token y cambiar contraseña
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.NuevaContrasena);

            if (!result.Succeeded)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al cambiar la contraseña",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            // 🔥 INVALIDAR TODAS LAS SESIONES
            await userManager.UpdateSecurityStampAsync(user);

            // --- NUEVO: limpiar cache del usuario al invalidar sesiones ---
            try
            {
                _cache.Remove($"UserSession_{user.Id}");
                _cache.Remove($"LastActivity_{user.Id}");
                _cache.Remove($"DbUpdate_{user.Id}");
            }
            catch
            {
            }

            // 🔐 Logout inmediato si es el mismo usuario
            var usuarioActualId = userManager.GetUserId(User);

            if (usuarioActualId == user.Id)
            {
                await signInManager.SignOutAsync();

                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Cambió de Contraseña de " + user.primerNombre + " " + user.primerApellido
                };
                _context.Auditoria.Add(auditoria);

                return Json(new
                {
                    success = true,
                    message = "Contraseña cambiada exitosamente. Por favor inicie sesión nuevamente.",
                    logout = true
                });
            }

            return Json(new
            {
                success = true,
                message = "Contraseña cambiada correctamente",
                logout = false
            });
        }




        // -------------------------------------------------------------------------------------------------------------------------------
        // ADMINISTRADOR

        [HttpGet]
        public async Task<IActionResult> MostrarUsers()
        {
            var usuarios = userManager.Users.ToList();
            var usuariosViewModel = new List<UsuarioViewModel>();

            foreach (var user in usuarios)
            {
                var rol = (await userManager.GetRolesAsync(user)).FirstOrDefault();

                usuariosViewModel.Add(new UsuarioViewModel
                {
                    Id = user.Id,
                    NombreCompleto = user.nombreCompleto,
                    UserName = user.UserName,
                    Email = user.Email,
                    RolNombre = rol ?? "Sin rol asignado",
                    Estado = user.estado
                });
            }

            return View("MostrarUsers", usuariosViewModel);
        }

        [HttpPost("Usuario/InactivarUs/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InactivarUs(string id)
        {
            try
            {
                var usuario = await userManager.FindByIdAsync(id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                usuario.estado = !usuario.estado;

                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var user = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = user.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Cambio de Estado de " + usuario.primerNombre + " " + usuario.primerApellido
                };
                _context.Auditoria.Add(auditoria);

                await _context.SaveChangesAsync();

                return Json(new { success = true, redirectUrl = Url.Action(nameof(MostrarUsers)) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Usuario/CambiarRol/{id}")]
        public async Task<ActionResult> CambiarRol(string id)
        {
            try
            {
                // Leer el JSON del body
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(body);

                string rolId = json.GetProperty("rolId").GetString();

                if (string.IsNullOrEmpty(rolId))
                {
                    return Json(new { success = false, message = "Debe seleccionar un rol" });
                }

                var usuario = await userManager.FindByIdAsync(id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                var rol = await roleManager.FindByIdAsync(rolId);
                if (rol == null)
                {
                    return Json(new { success = false, message = "Rol no encontrado" });
                }

                // Quitar roles actuales
                var rolesActuales = await userManager.GetRolesAsync(usuario);
                if (rolesActuales.Any())
                {
                    var removeResult = await userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                    if (!removeResult.Succeeded)
                    {
                        return Json(new { success = false, message = "Error al remover roles actuales" });
                    }
                }

                // Asignar nuevo rol
                var addResult = await userManager.AddToRoleAsync(usuario, rol.Name);
                if (!addResult.Succeeded)
                {
                    return Json(new { success = false, message = "Error al asignar el nuevo rol" });
                }

                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var user = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = user.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Cambió de Rol por " + rol.Name + " a " + usuario.primerNombre + " " + usuario.primerApellido
                };
                _context.Auditoria.Add(auditoria);

                return Json(new
                {
                    success = true,
                    message = "Rol cambiado correctamente",
                    redirectUrl = Url.Action(nameof(MostrarUsers))
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error inesperado al cambiar rol: " + ex.Message });
            }
        }

        [Authorize(Policy = "Usuarios.Crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(
            string primerNombre,
            string segundoNombre,
            string primerApellido,
            string segundoApellido,
            string correo,
            string contrasena,
            string rolId)
        {
            try
            {
                // 1. Validaciones básicas
                if (string.IsNullOrWhiteSpace(primerNombre))
                    return Json(new { success = false, message = "El primer nombre es obligatorio" });

                if (string.IsNullOrWhiteSpace(primerApellido))
                    return Json(new { success = false, message = "El primer apellido es obligatorio" });

                if (string.IsNullOrWhiteSpace(correo))
                    return Json(new { success = false, message = "El correo electrónico es obligatorio" });

                if (string.IsNullOrWhiteSpace(contrasena))
                    return Json(new { success = false, message = "La contraseña es obligatoria" });

                if (contrasena.Length < 8)
                    return Json(new { success = false, message = "La contraseña debe tener al menos 8 caracteres" });

                // 2. Validar formato de email
                var emailValidator = new EmailAddressAttribute();
                if (!emailValidator.IsValid(correo))
                    return Json(new { success = false, message = "El formato del correo electrónico no es válido" });

                // 3. Verificar si el correo ya existe
                var usuarioExistente = await userManager.FindByEmailAsync(correo);
                if (usuarioExistente != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "El correo electrónico ya está registrado"
                    });
                }

                // 4. Crear el usuario con los datos del modal (simplificado)
                var nuevoUsuario = new Usuario
                {
                    primerNombre = primerNombre,
                    segundoNombre = segundoNombre,
                    primerApellido = primerApellido,
                    segundoApellido = segundoApellido,
                    correoEmpresa = correo,
                    Email = correo,
                    UserName = correo,
                    estado = true
                };

                // 5. Crear usuario con la contraseña proporcionada (NO fija)
                var resultado = await userManager.CreateAsync(nuevoUsuario, contrasena);

                if (!resultado.Succeeded)
                {
                    var errors = resultado.Errors.Select(e => e.Description).ToList();
                    var errorMessage = errors.Any() ?
                        string.Join(", ", errors) : "Error al crear el usuario";

                    return Json(new
                    {
                        success = false,
                        message = errorMessage
                    });
                }

                // 6. Asignar rol
                if (!string.IsNullOrWhiteSpace(rolId))
                {
                    // Buscar el rol por ID
                    var role = await roleManager.FindByIdAsync(rolId);
                    if (role != null)
                    {
                        await userManager.AddToRoleAsync(nuevoUsuario, role.Name);
                    }
                    else
                    {
                        // Si no encuentra el rol por ID, usar uno por defecto
                        await userManager.AddToRoleAsync(nuevoUsuario, "Empleado Normal");
                    }
                }
                else
                {
                    // Si no se seleccionó rol, asignar por defecto
                    await userManager.AddToRoleAsync(nuevoUsuario, "Empleado Normal");
                }

                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"           // Windows
                    : "America/Costa_Rica";                     // Linux/macOS

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);
                DateOnly hoy = DateOnly.FromDateTime(ahoraCR);
                var usuario = await userManager.GetUserAsync(User);

                // Auditoría
                var auditoria = new Auditoria
                {
                    Fecha = hoy,
                    Hora = TimeOnly.FromDateTime(ahoraCR).ToTimeSpan(),
                    Usuario = usuario.nombreCompleto ?? "Desconocido",
                    Tabla = "Usuario",
                    Accion = "Nuevo registro por parte de Admin"
                };
                _context.Auditoria.Add(auditoria);

                return Json(new
                {
                    success = true,
                    message = "Usuario creado exitosamente",
                    usuario = new
                    {
                        nombre = $"{primerNombre} {primerApellido}",
                        email = correo
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error interno del servidor. Por favor, intente nuevamente."
                });
            }
        }
    }
}
