using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.DTO;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;
using System.Runtime.InteropServices;

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
        public ActionResult Index()
        {
            return View();
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
        public ActionResult Details(int id)
        {
            return View();
        }

        // CREATE = REGISTRAR EMPLEADO CON FORMULARIO Y TODO
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: UsuarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuarioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
    }
}
