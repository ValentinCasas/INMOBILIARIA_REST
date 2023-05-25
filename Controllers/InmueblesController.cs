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
using System.Globalization;

using System.Diagnostics;

namespace INMOBILIARIA_REST.Controllers;

[Authorize]
public class InmueblesController : Controller
{


        private readonly IWebHostEnvironment environment;

    public InmueblesController(IWebHostEnvironment environment)
{
    this.environment = environment;
}


    public IActionResult Index()
    {
        try
        {

            RepositorioInmueble repositorioInmueble = new RepositorioInmueble();

            ViewBag.lista = repositorioInmueble.GetInmuebles();
            ViewBag.contratos = repositorioInmueble.GetContratos();

            return View();
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    public IActionResult Update()
    {
        return View();
    }

    public IActionResult Create()
    {
        RepositorioInmueble repositorioInmueble = new RepositorioInmueble();
        ViewBag.listaPropietarios = repositorioInmueble.GetPropietarios();
        return View();
    }

    [HttpPost]
    public IActionResult Create(Inmueble inmueble)
    {
        try
        {
            RepositorioInmueble repositorioInquilino = new RepositorioInmueble();
                if (inmueble.AvatarFile == null || inmueble.AvatarFile.Length == 0)
                {
                    inmueble.AvatarUrl = "/Imagenes/avatar_por_defecto_propiedad.jpg";
                    repositorioInquilino.Alta(inmueble);
                    return RedirectToAction("Create");
                }
                else
                {
                    string wwwPath = environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads");

                    string fileName = "avatar_" + Guid.NewGuid().ToString("N") + Path.GetExtension(inmueble.AvatarFile.FileName);
                    string pathCompleto = Path.Combine(path, fileName);
                    inmueble.AvatarUrl = Path.Combine("/Uploads", fileName);


                        // Esta operación guarda la foto en memoria en la ruta que necesitamos
                        using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                        {
                            inmueble.AvatarFile.CopyTo(stream);
                        }
                        repositorioInquilino.Alta(inmueble);
             return RedirectToAction("Create");
                }
         
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Create");
        }
    }


    [HttpGet]
    public IActionResult Delete(int id)
    {
        try
        {
            RepositorioInmueble repositorioInmueble = new RepositorioInmueble();
            Inmueble inmueble = repositorioInmueble.ObtenerPorId(id);
            Boolean res = repositorioInmueble.Baja(id);
            if (res == true)
            {
                 try
            {
                var ruta = Path.Combine(environment.WebRootPath, inmueble.AvatarUrl.TrimStart('/').Replace('/', '\\'));

                
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

                TempData["Error"] = "Ocurrió un error al intentar eliminar el avatar del inmueble.";
                return RedirectToAction("Index");
            }
                return RedirectToAction("index");
            }
            else
            {
                TempData["Error"] = "Ocurrió un error al intentar eliminar el inmueble.";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo borrar el inmueble, para mas informacion contactarse con el administrador, probablemente este asociado a algun contrato";
            return RedirectToAction("Index");
        }
    }


    [HttpGet]
    public IActionResult Update(int id)
    {
        try
        {
            RepositorioInmueble repositorioInmueble = new RepositorioInmueble();
            Inmueble inmueble = repositorioInmueble.ObtenerPorId(id);

            if (inmueble != null)
            {
                ViewBag.listaPropietarios = repositorioInmueble.GetPropietarios();
                ViewBag.inmueble = inmueble;
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
    public IActionResult UpdateInmueble(Inmueble inmueble)
    {
        try
        {
            RepositorioInmueble repositorioInmueble = new RepositorioInmueble();
            Inmueble u = repositorioInmueble.ObtenerPorId(inmueble.Id);

             if (inmueble.AvatarFile == null || inmueble.AvatarFile.Length == 0)
                    {
                        inmueble.AvatarUrl = u.AvatarUrl;
                    }else
                    {
                        string wwwPath = environment.WebRootPath;
                        string path = Path.Combine(wwwPath, "Uploads");
                        string fileName = "avatar_" + Guid.NewGuid().ToString("N") + Path.GetExtension(inmueble.AvatarFile.FileName);
                        string pathCompleto = Path.Combine(path, fileName);
                        inmueble.AvatarUrl = Path.Combine("/Uploads", fileName);

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
                            inmueble.AvatarFile.CopyTo(stream);
                        }
                    }

            Boolean res = repositorioInmueble.Actualizar(inmueble);
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

    [HttpPost]
    public IActionResult InmueblePorFecha(string fechaInicio, string fechaFin)
    {
        try
        {
            DateTime fechaInicioDateTime = DateTime.ParseExact(fechaInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime fechaFinDateTime = DateTime.ParseExact(fechaFin, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            RepositorioInmueble repositorioInmueble = new RepositorioInmueble();
            List<Inmueble> inmuebles = repositorioInmueble.BuscarInmueblesPorFecha(fechaInicioDateTime, fechaFinDateTime);

            if (inmuebles != null)
            {
                ViewBag.lista = inmuebles;
                ViewBag.contratos = repositorioInmueble.GetContratos();
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            return RedirectToAction("Index");
        }
    }



}