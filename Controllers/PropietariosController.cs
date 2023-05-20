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
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;


namespace INMOBILIARIA_REST.Controllers
{
    /* [Authorize(Policy = "Administrador")] */
    [Authorize]
    public class PropietariosController : Controller
    {

        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        public PropietariosController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            RepositorioPropietario repositorioPropietario = new RepositorioPropietario();
            ViewBag.lista = repositorioPropietario.GetPropietarios();
            ViewBag.inmuebles = repositorioPropietario.GetInmuebles();

            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Propietario propietarioForm)
        {
            Console.WriteLine(propietarioForm.ToString());
            try
            {
                RepositorioPropietario repositorioPropietario = new RepositorioPropietario();
                Propietario propietario = repositorioPropietario.ObtenerPorEmail(propietarioForm.Email);


                if (propietario != null)
                {
                    TempData["Error"] = "ya hay un propietario con este email:" + " " + propietarioForm.Email;
                    return RedirectToAction("Create");
                }

                bool contrasena = VerificarContrasena(propietarioForm.Clave);
                if (contrasena == false)
                {
                    TempData["Error"] = "Su contraseña debe tener al menos 8 caracteres, una mayuscula, y un numero";
                    return RedirectToAction("Create");
                }

                if (propietarioForm.ConfirmarClave != propietarioForm.Clave)
                {
                    TempData["Error"] = "Las contraseñas no coinciden";
                    return RedirectToAction("Create");
                }

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                 password: propietarioForm.Clave,
                                 salt: System.Text.Encoding.ASCII.GetBytes(configuration["Salt"]),
                                 prf: KeyDerivationPrf.HMACSHA1,
                                 iterationCount: 30000,
                                 numBytesRequested: 256 / 8));
                propietarioForm.Clave = hashed;


                int res = repositorioPropietario.Alta(propietarioForm);
                if (res > 0)
                {
                    return RedirectToAction("index");
                }
                else
                {
                    TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Create");
            }
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


        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                RepositorioPropietario repositorioPropietario = new RepositorioPropietario();
                Boolean res = repositorioPropietario.Baja(id);
                if (res == true)
                {
                    return RedirectToAction("index");
                }
                else
                {
                    TempData["Error"] = "Ocurrió un error al intentar eliminar el propietario.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo borrar el propietario, probablemente este asociado a algun inmueble";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            try
            {
                RepositorioPropietario repositorioPropietario = new RepositorioPropietario();
                ViewBag.propietario = repositorioPropietario.ObtenerPorId(id);
                if (ViewBag.propietario != null)
                {
                    return View("Update");
                }
                else
                {
                    TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult UpdatePropietario(Propietario propietario)
        {
            try
            {
                RepositorioPropietario respositorioPersona = new RepositorioPropietario();
                 Propietario u = respositorioPersona.ObtenerPorId(propietario.Id);


                if (propietario.ClaveAntigua != null && propietario.NuevaClave != null && propietario.ConfirmarClave != null)
                {

                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                     password: propietario.ClaveAntigua,
                                     salt: System.Text.Encoding.ASCII.GetBytes(configuration["Salt"]),
                                     prf: KeyDerivationPrf.HMACSHA1,
                                     iterationCount: 30000,
                                     numBytesRequested: 256 / 8));

                    if (propietario.Clave != hashed)
                    {
                        TempData["Error"] = "la clave antigua ingresada no es correcta";
                        return RedirectToAction("Update", new { id = propietario.Id });
                    }

                    bool contrasena = VerificarContrasena(propietario.NuevaClave);
                    if (contrasena == false)
                    {
                        TempData["Error"] = "Su contraseña debe tener al menos 8 caracteres, una mayuscula, y un numero";
                        return RedirectToAction("Update", new { id = propietario.Id });
                    }
                    else
                    {
                        if (propietario.NuevaClave != propietario.ConfirmarClave)
                        {
                            TempData["Error"] = "La nueva contraseña ingresada no coincide con la confirmacion";
                            return RedirectToAction("Update", new { id = propietario.Id });
                        }
                    }

                    string claveNuevaHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                     password: propietario.NuevaClave,
                                     salt: System.Text.Encoding.ASCII.GetBytes(configuration["Salt"]),
                                     prf: KeyDerivationPrf.HMACSHA1,
                                     iterationCount: 30000,
                                     numBytesRequested: 256 / 8));
                    propietario.Clave = claveNuevaHashed;

                }


                if (propietario.Email != u.Email)
        {
            List<Propietario> propietarioEmail = respositorioPersona.ObtenerPorEmailList(propietario.Email);
            if (propietarioEmail.Count >= 1)
            {
                TempData["Error"] = "ya hay un usuario con este email";
                return RedirectToAction("Update", new { id = propietario.Id });
            }
        }






                    Boolean res = respositorioPersona.Actualizar(propietario);
                    if (res == true)
                    {
                        return RedirectToAction("index");
                    }
                    else
                    {
                        TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                        return RedirectToAction("Update");
                    }
                }
            catch (Exception ex)
            {
                TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                return RedirectToAction("Update");
            }
        }
    }
}
