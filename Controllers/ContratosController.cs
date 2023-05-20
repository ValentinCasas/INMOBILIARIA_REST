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
using System.Globalization;

namespace INMOBILIARIA_REST.Controllers;

[Authorize]
public class ContratosController : Controller
{

    public ContratosController() { }

    public IActionResult Index()
    {
        RepositorioContrato repositorioContrato = new RepositorioContrato();
        ViewBag.lista = repositorioContrato.GetContratos();
        ViewBag.pagos = repositorioContrato.GetPagos();
        return View();
    }

    [HttpGet]
    public IActionResult Create(int? inmuebleId)
    {
        RepositorioContrato repositorioContrato = new RepositorioContrato();
        ViewBag.InmuebleId = inmuebleId;
        ViewBag.listaInquilinos = repositorioContrato.GetInquilinos();
        ViewBag.listaInmuebles = repositorioContrato.GetInmuebles();
        return View();
    }


    [HttpPost]
    public IActionResult Create(Contrato contrato)
    {
        try
        {

            if (contrato.FechaInicio >= contrato.FechaFinalizacion)
            {
                TempData["Error"] = "La fecha de inicio debe ser anterior a la fecha de finalización";
                return RedirectToAction("Create");
            }

            if (contrato.FechaInicio < DateTime.Now.Date)
            {
                TempData["Error"] = "La fecha de inicio debe ser igual o posterior a la fecha actual";
                return RedirectToAction("Create");
            }

            RepositorioContrato repositorioContrato = new RepositorioContrato();

            if (repositorioContrato.ExisteSolapamientoContratosActivos(contrato.IdInmueble, contrato.FechaInicio, contrato.FechaFinalizacion))
            {
                TempData["Error"] = "Ya existe un contrato activo en el mismo inmueble que se solapa con las fechas del nuevo contrato";
                return RedirectToAction("Create");
            }

            int res = repositorioContrato.Alta(contrato);

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
            RepositorioContrato repositorioContrato = new RepositorioContrato();
            Boolean res = repositorioContrato.Baja(id);
            if (res == true)
            {

                return RedirectToAction("index");
            }
            else
            {
                TempData["Error"] = "Ocurrió un error al intentar eliminar el contrato, probablemente este asociado a algun pago.";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Ocurrió un error al intentar eliminar el contrato, probablemente este asociado a algun pago.";
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public IActionResult Update(int id)
    {
        try
        {
            RepositorioContrato repositorioContrato = new RepositorioContrato();
            Contrato contrato = repositorioContrato.ObtenerPorId(id);
            if (contrato != null)
            {
                ViewBag.listaInquilinos = repositorioContrato.GetInquilinos();
                ViewBag.listaInmuebles = repositorioContrato.GetInmuebles();
                ViewBag.contrato = contrato;
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
public IActionResult UpdateContrato(Contrato contrato)
{
    Boolean res;
    RepositorioContrato repositorioContrato = new RepositorioContrato();
    try
    {

         int cantidadQueRepite = repositorioContrato.ContarSolapamientoContratosActivos(contrato.IdInmueble, contrato.FechaInicio, contrato.FechaFinalizacion, contrato.Id);

        if (cantidadQueRepite >= 1)
        {
            TempData["Error"] = "Ya existe un contrato activo en el mismo inmueble que se solapa con las fechas del nuevo contrato";
            return RedirectToAction("Update", new { id = contrato.Id });
        }



            if (contrato.FechaFinalizacion < contrato.FechaInicio)
            {
                TempData["Error"] = "La fecha de finalización no puede ser anterior a la fecha de inicio";
                return RedirectToAction("Update", new { id = contrato.Id });
            }
            else if (contrato.FechaInicio > DateTime.Now.Date && (contrato.Activo || !contrato.Activo))
            {
                // Si la fecha de inicio es posterior a la fecha actual y el contrato está activo o inactivo,
                // no se agrega ninguna multa y se actualiza perfectamente
                res = repositorioContrato.Actualizar(contrato);
                if (res == true)
                {
                    return RedirectToAction("index");
                }
                else
                {
                    TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                    return RedirectToAction("Update", new { id = contrato.Id });
                }
            }
            else
            {
                TimeSpan tiempoTranscurrido = DateTime.Now.Date - contrato.FechaInicio;
                if (contrato.FechaFinalizacion < DateTime.Now.Date && !contrato.Activo)
                {
                    contrato.Activo = false;
                    // Si la fecha de finalización es anterior a la fecha actual, se debe calcular la multa
                    TimeSpan tiempoAlquiler = contrato.FechaFinalizacion - contrato.FechaInicio;
                    // Actualizar el tiempo transcurrido para tener en cuenta la fecha de finalización
                    tiempoTranscurrido = contrato.FechaFinalizacion - contrato.FechaInicio;

                    // Se verifica si se cumplió menos de la mitad del tiempo original de alquiler
                    if (tiempoTranscurrido.TotalDays < (tiempoAlquiler.TotalDays / 2))
                    {
                    }
                    TempData["Mensaje"] = "Debe pagar 2 (dos) meses extra de alquiler: $" + contrato.MontoAlquilerMensual * 2;
                    repositorioContrato.AgregarMulta(contrato.IdInquilino, contrato.MontoAlquilerMensual * 2);
                }
                else if (contrato.FechaFinalizacion >= DateTime.Now.Date && !contrato.Activo)
                {
                    TempData["Mensaje"] = "Debe pagar 1 (un) mes extra de alquiler: $" + contrato.MontoAlquilerMensual;
                    repositorioContrato.AgregarMulta(contrato.IdInquilino, contrato.MontoAlquilerMensual);
                }
                if (contrato.Activo)
                {
                    TempData.Remove("Error"); // Se elimina el mensaje de error si el contrato está activo
                }
            }

            res = repositorioContrato.Actualizar(contrato);
            if (res == true)
            {
                return RedirectToAction("index");
            }
            else
            {
                TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
                return RedirectToAction("Update", new { id = contrato.Id });
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Por favor llene todos los campos y ponga los datos correctamente";
            return RedirectToAction("Update", new { id = contrato.Id });
        }
    }

    [HttpPost]
    public IActionResult ContratoPorFecha(string fechaInicio, string fechaFin)
    {
        try
        {
            DateTime fechaInicioDateTime = DateTime.ParseExact(fechaInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime fechaFinDateTime = DateTime.ParseExact(fechaFin, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (fechaInicioDateTime > fechaFinDateTime)
            {
                TempData["Error"] = "La fecha de inicio debe ser posterior a la fecha final";
                return RedirectToAction("Index");
            }

            RepositorioContrato repositorioContrato = new RepositorioContrato();
            List<Contrato> contratos = repositorioContrato.ObtenerContratosVigentes(fechaInicioDateTime, fechaFinDateTime);


            if (contratos.Count > 0)
            {
                ViewBag.lista = contratos;
                ViewBag.pagos = repositorioContrato.GetPagos();
                return View();
            }
            else
            {
                TempData["Error"] = "No se encontraron contratos vigentes entre las fechas: " + fechaInicioDateTime + " / " + fechaFinDateTime;
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al encontrar contratos";
            return RedirectToAction("Index");
        }
    }



}
