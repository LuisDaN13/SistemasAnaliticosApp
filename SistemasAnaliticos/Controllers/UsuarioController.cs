using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using System.Runtime.InteropServices;
using System.Text;
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

        // INDEX = PRESENTAR A LOS EMPLEADOS CON CARDS PARA SCINICIO DE SESIÓN DE LA APLICACIÓN CON CORREO Y CONTRASEÑA
        public async Task<ActionResult> Index()
        {
            var cards = await _context.Users
                .AsNoTracking()
                .OrderByDescending(x => x.primerNombre)
                .Select(x => new CardsDTO
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


        // LOGIN = INICIO DE SESIÓN DE LA APLICACIÓN CON CORREO Y CONTRASEÑA
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
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


        // DETAILS = VER MÁS INFORMACIÓN DEL EMPLEADO
        public async Task<ActionResult> Details(string id)
        {
            var details = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return View(details);

        }


        // CREATE = REGISTRAR EMPLEADO CON FORMULARIO Y TODO
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Usuario model)
        {
            try
            {
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
                    Email = model.correoEmpresa,
                    UserName = model.correoEmpresa,

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
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

            }
            catch
            {
                return RedirectToAction("Usuario", "Index");
            }
        }


        // GET: UsuarioController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }


        // POST: UsuarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inactivar(long id)
        {
            var usuario = _context.Users.Find(id);

            usuario.estado = false;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
