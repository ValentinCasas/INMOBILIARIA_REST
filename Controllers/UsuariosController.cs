using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using INMOBILIARIA_REST.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace INMOBILIARIA_REST.Controllers;


public class UsuariosController : Controller
{
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;

    public UsuariosController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        this.configuration = configuration;
        this.environment = environment;
    }

    [Authorize(Policy = "Administrador")]
    public ActionResult Index()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();
        ViewBag.lista = repositorio.GetUsuarios();
        ViewBag.Roles = Usuario.ObtenerRoles();
        return View();
    }


    [Authorize]
    [HttpGet]
    public ActionResult Profile(int id)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();
        ViewBag.MyProfile = repositorio.ObtenerPorId(id);
        ViewBag.Roles = Usuario.ObtenerRoles();
        return View();
    }


    /* [Authorize(Policy = "Administrador")] */
    [HttpGet]
    [Authorize]
    public IActionResult Update(int id)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();

        Usuario usuario = repositorio.ObtenerPorId(id);

        if (User.IsInRole("Administrador") || User.FindFirst("EmpleadoId").Value == usuario.Id.ToString())
        {
            ViewBag.Usuario = usuario;
            ViewBag.Roles = Usuario.ObtenerRoles();
            return View();
        }
        else
        {
            return RedirectToAction("Update", new { id = User.FindFirst("EmpleadoId").Value });
        }
    }


    /* [Authorize(Policy = "Administrador")] */
    [Authorize]
    [HttpPost]
    public IActionResult UpdateUsuario(Usuario usuario)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();
        Usuario u = repositorio.ObtenerPorId(usuario.Id);

        // Comprobar si el usuario es el propietario o tiene rol de administrador
        if (User.FindFirst("EmpleadoId").Value != usuario.Id.ToString() && !User.IsInRole("Administrador"))
        {
            TempData["Error"] = "No tiene permiso para realizar esta acción";
            return RedirectToAction("Update", new { id = User.FindFirst("EmpleadoId").Value });
        }

        if (usuario.AvatarFile == null || usuario.AvatarFile.Length == 0)
        {
            usuario.AvatarUrl = u.AvatarUrl;
        }
        else
        {
            string wwwPath = environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Uploads");
            string fileName = "avatar_" + Guid.NewGuid().ToString("N") + Path.GetExtension(usuario.AvatarFile.FileName);
            string pathCompleto = Path.Combine(path, fileName);
            usuario.AvatarUrl = Path.Combine("/Uploads", fileName);

            var ruta = Path.Combine(environment.WebRootPath, u.AvatarUrl.TrimStart('/').Replace('/', '\\'));

            // Verifica que la ruta comience con "/Uploads"
            if (ruta.StartsWith(Path.Combine(environment.WebRootPath, "Uploads")))
            {
                if (System.IO.File.Exists(ruta))
                {
                    System.IO.File.Delete(ruta);
                }
            }

            using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
            {
                usuario.AvatarFile.CopyTo(stream);
            }
        }

        if (usuario.ClaveAntigua != null && usuario.NuevaClave != null && usuario.ConfirmarClave != null)
        {

            string salt = "palabrasecretaparalacontraseña";
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                             password: usuario.ClaveAntigua,
                             salt: System.Text.Encoding.ASCII.GetBytes(salt),
                             prf: KeyDerivationPrf.HMACSHA1,
                             iterationCount: 30000,
                             numBytesRequested: 256 / 8));

            if (usuario.Clave != hashed)
            {
                TempData["Error"] = "la clave antigua ingresada no es correcta";
                return RedirectToAction("Update", new { id = usuario.Id });
            }

            bool contrasena = VerificarContrasena(usuario.NuevaClave);
            if (contrasena == false)
            {
                TempData["Error"] = "Su contraseña debe tener al menos 8 caracteres, una mayuscula, y un numero";
                return RedirectToAction("Update", new { id = usuario.Id });
            }
            else
            {
                if (usuario.NuevaClave != usuario.ConfirmarClave)
                {
                    TempData["Error"] = "La nueva contraseña ingresada no coincide con la confirmacion";
                    return RedirectToAction("Update", new { id = usuario.Id });
                }
            }

            string claveNuevaHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                             password: usuario.NuevaClave,
                             salt: System.Text.Encoding.ASCII.GetBytes(salt),
                             prf: KeyDerivationPrf.HMACSHA1,
                             iterationCount: 30000,
                             numBytesRequested: 256 / 8));
            usuario.Clave = claveNuevaHashed;

        }


        if (usuario.Email != u.Email)
        {
            List<Usuario> usuariosEmail = repositorio.ObtenerPorEmailList(usuario.Email);
            if (usuariosEmail.Count >= 1)
            {
                TempData["Error"] = "ya hay un usuario con este email";
                return RedirectToAction("Update", new { id = usuario.Id });
            }
        }

        Boolean res = repositorio.Actualizar(usuario);

        return RedirectToAction("Update", new { id = usuario.Id });

    }


    [Authorize(Policy = "Administrador")]
    public ActionResult Create()
    {
        ViewBag.Roles = Usuario.ObtenerRoles();
        return View();
    }

    [Authorize(Policy = "Administrador")]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();
        Usuario usuario = repositorio.ObtenerPorId(id);

        bool res = repositorio.Baja(id);
        if (res)
        {
            try
            {
                var ruta = Path.Combine(environment.WebRootPath, usuario.AvatarUrl.TrimStart('/').Replace('/', '\\'));

                // Verifica que la ruta comience con "/Uploads"
                if (ruta.StartsWith(Path.Combine(environment.WebRootPath, "Uploads")))
                {
                    if (System.IO.File.Exists(ruta))
                    {
                        System.IO.File.Delete(ruta);
                    }
                }
            }
            catch (Exception ex)
            {

                TempData["Error"] = "Ocurrió un error al intentar eliminar el avatar del usuario.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
        else
        {
            TempData["Error"] = "Ocurrió un error al intentar eliminar el usuario.";
            return RedirectToAction("Index");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Administrador")]
    public ActionResult Create(Usuario u)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();

        Usuario usuario = repositorio.ObtenerPorEmail(u.Email);

        if (usuario != null)
        {
            TempData["Error"] = "ya hay un usuario con este email:" + " " + u.Email;
            return RedirectToAction("Create");
        }

        bool contrasena = VerificarContrasena(u.Clave);
        if (contrasena == false)
        {
            TempData["Error"] = "Su contraseña debe tener al menos 8 caracteres, una mayuscula, y un numero";
            return RedirectToAction("Create");
        }

        if (u.ConfirmarClave != u.Clave)
        {
            TempData["Error"] = "Las contraseñas no coinciden";
            return RedirectToAction("Create");
        }

        try
        {

            string salt = "palabrasecretaparalacontraseña";
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                             password: u.Clave,
                             salt: System.Text.Encoding.ASCII.GetBytes(salt),
                             prf: KeyDerivationPrf.HMACSHA1,
                             iterationCount: 30000,
                             numBytesRequested: 256 / 8));
            u.Clave = hashed;
            u.Rol = User.IsInRole("Administrador") ? u.Rol : (int)enRoles.Empleado;



            if (u.AvatarFile == null || u.AvatarFile.Length == 0)
            {
                u.AvatarUrl = "/Imagenes/avatar_por_defecto.jpg";
                repositorio.Alta(u);
                return RedirectToAction("Create");
            }
            else
            {
                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "Uploads");

                string fileName = "avatar_" + Guid.NewGuid().ToString("N") + Path.GetExtension(u.AvatarFile.FileName);
                string pathCompleto = Path.Combine(path, fileName);
                u.AvatarUrl = Path.Combine("/Uploads", fileName);

                int res = repositorio.Alta(u);
                if (res > 0)
                {
                    // Esta operación guarda la foto en memoria en la ruta que necesitamos
                    using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                    {
                        u.AvatarFile.CopyTo(stream);
                    }
                    return RedirectToAction("Create");
                }

            }


        }
        catch (Exception ex)
        {
            TempData["Error"] = "error al cargar el usuario, asegurese de llenar todos los campos obligatorios";
            return RedirectToAction("Create");
        }

        ViewBag.Roles = Usuario.ObtenerRoles();
        return View(u);
    }
    public static bool VerificarContrasena(string contrasena)
    {
        if (contrasena.Length < 8)
        {
            return false;
        }
        bool tieneMayuscula = false;
        bool tieneNumero = false;
        foreach (char caracter in contrasena)
        {
            if (char.IsUpper(caracter))
            {
                tieneMayuscula = true;
            }
            if (char.IsDigit(caracter))
            {
                tieneNumero = true;
            }
            if (tieneMayuscula && tieneNumero)
            {
                return true;
            }
        }
        return false;
    }

    [AllowAnonymous]
    // GET: Usuarios/Login/
    public ActionResult Login(string returnUrl)
    {
        try
        {
            TempData["returnUrl"] = returnUrl;
            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }


    // POST: Usuarios/Login/
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginView login)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario();
        try
        {
            var returnUrl = String.IsNullOrEmpty(TempData["returnUrl"] as string) ? "/Home" : TempData["returnUrl"].ToString();

            if (ModelState.IsValid)
            {
                var e = repositorio.ObtenerPorEmail(login.Usuario);

                if (e == null)
                {
                    ModelState.AddModelError("", "El email o la clave no son correctos");
                    TempData["returnUrl"] = returnUrl;
                    return View();
                }

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                 password: login.Clave,
                                 salt: System.Text.Encoding.ASCII.GetBytes(configuration["Salt"]),
                                 prf: KeyDerivationPrf.HMACSHA1,
                                 iterationCount: 30000,
                                 numBytesRequested: 256 / 8));

                if (e.Clave != hashed)
                {
                    ModelState.AddModelError("", "El email o la clave no son correctos");
                    TempData["returnUrl"] = returnUrl;
                    return View();
                }

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, e.Email),
                new Claim("FullName", e.Nombre + " " + e.Apellido),
                new Claim(ClaimTypes.Role, e.RolNombre),
                new Claim("EmpleadoId", e.Id.ToString()),

            };

                var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                TempData.Remove("returnUrl");
                return Redirect(returnUrl);
            }
            TempData["returnUrl"] = returnUrl;
            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }



    // GET: /salir
    [Route("salir", Name = "logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }


}