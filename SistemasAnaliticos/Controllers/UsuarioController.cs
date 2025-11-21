using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.ViewModels;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using System.Runtime.InteropServices;
using static SistemasAnaliticos.Models.codigoFotos;

namespace SistemasAnaliticos.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly SignInManager<Usuario> signInManager;
        private readonly UserManager<Usuario> userManager;
        private readonly DBContext _context;

        public UsuarioController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager, DBContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // INDEX = PRESENTAR A LOS EMPLEADOS CON CARDS PARA SCINICIO DE SESIÓN DE LA APLICACIÓN CON CORREO Y CONTRASEÑA
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
        // LOGIN = INICIO DE SESIÓN DE LA APLICACIÓN CON CORREO Y CONTRASEÑA
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Detectar sistema operativo y usar el ID de zona horaria adecuado
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Central America Standard Time"
                    : "America/Costa_Rica";

                TimeZoneInfo zonaCR = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);

                // VERIFICAR Y INVALIDAR SESIONES ANTERIORES
                var usuario = await userManager.FindByEmailAsync(model.Email);
                if (usuario != null)
                {
                    // Invalidar todas las sesiones activas anteriores
                    var sesionesActivas = _context.UsuarioSesion
                        .Where(s => s.UserId == usuario.Id && s.IsActive);

                    foreach (var sesion in sesionesActivas)
                    {
                        sesion.IsActive = false;
                    }
                    await _context.SaveChangesAsync();
                }

                // INTENTAR LOGIN
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

                if (result.Succeeded)
                {
                    // OBTENER USUARIO LOGUEADO
                    var user = await userManager.FindByEmailAsync(model.Email);

                    // CREAR NUEVA SESIÓN
                    var nuevaSesion = new UsuarioSesion
                    {
                        UserId = user.Id,
                        SessionId = Guid.NewGuid().ToString(),
                        LoginDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.UsuarioSesion.Add(nuevaSesion);
                    await _context.SaveChangesAsync();

                    // ALMACENAR EN SESSION Y COOKIE
                    HttpContext.Session.SetString("correoLogeado", model.Email);
                    HttpContext.Session.SetString("UltimaActividad", ahoraCR.ToString());
                    HttpContext.Session.SetString("SessionId", nuevaSesion.SessionId);

                    // Cookie adicional para el middleware
                    Response.Cookies.Append("CurrentSessionId", nuevaSesion.SessionId, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Intento de Inicio de Sesión Fallido.");
                    return View(model);
                }
            }
            return View(model);
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // DETAILS = VER MÁS INFORMACIÓN DEL EMPLEADO
        public async Task<ActionResult> Details(string id)
        {
            var details = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return View(details);

        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // CREATE = REGISTRAR EMPLEADO CON FORMULARIO Y TODO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Usuario model)
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
                    cedula = model.cedula,
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

                    celularOficina = model.celularOficina,
                    jefe = model.jefe,
                    extension = model.extension,
                    salario = model.salario,
                    cuentaIBAN = model.cuentaIBAN,
                    celularPersonal = model.celularPersonal,
                    correoPersonal = model.correoPersonal,
                    telefonoHabitacion = model.telefonoHabitacion,

                    licencias = model.licencias,
                    tipoPariente = model.tipoPariente,
                    contactoEmergencia = model.contactoEmergencia,
                    telefonoEmergencia = model.telefonoEmergencia,
                    padecimientosAlergias = model.padecimientosAlergias,
                    estado = true,

                    foto = fotoService.ConvertFileToByteArrayAsync(model.fotoFile).Result
                };

                var pass = "123456789";

                var resultado = await userManager.CreateAsync(nuevo, pass);
                if (resultado.Succeeded)
                {
                    TempData["SuccessMessage"] = "El empleado se creó correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error en la creación del empleado.";
                    return RedirectToAction("Index", "Usuario");
                }

            }
            catch
            {
                TempData["ErrorMessage"] = "Error en el proceso.";
                return RedirectToAction("Usuario", "Index");
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // EDIT = MODIFICAR INFORMACIÓN DEL EMPLEADO
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
                usuario.cedula = model.cedula;
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

                usuario.celularOficina = model.celularOficina;
                usuario.jefe = model.jefe;
                usuario.extension = model.extension;
                usuario.salario = model.salario;
                usuario.cuentaIBAN = model.cuentaIBAN;
                usuario.celularPersonal = model.celularPersonal;
                usuario.correoPersonal = model.correoPersonal;
                usuario.telefonoHabitacion = model.telefonoHabitacion;

                usuario.licencias = model.licencias;
                usuario.tipoPariente = model.tipoPariente;
                usuario.contactoEmergencia = model.contactoEmergencia;
                usuario.telefonoEmergencia = model.telefonoEmergencia;
                usuario.padecimientosAlergias = model.padecimientosAlergias;

                // 🔹 Foto (solo si subió una nueva)
                if (model.fotoFile != null)
                {
                    usuario.foto = await fotoService.ConvertFileToByteArrayAsync(model.fotoFile);
                }

                // 🔹 Guardar cambios
                _context.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "El empleado se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
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
                await _context.SaveChangesAsync();

                return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------
        // LOG OUT
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Limpiar la sesión
            return RedirectToAction("Login", "Usuario");
        }
    }
}
