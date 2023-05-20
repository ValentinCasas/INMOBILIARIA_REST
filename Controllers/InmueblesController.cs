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

    public InmueblesController() { }

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
            int res = repositorioInquilino.Alta(inmueble);
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
            TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
            return RedirectToAction("Create");
        }
    }


    [HttpGet]
    public IActionResult Delete(int id)
    {
        try
        {
            RepositorioInmueble repositorioInmueble = new RepositorioInmueble();
            Boolean res = repositorioInmueble.Baja(id);
            if (res == true)
            {
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