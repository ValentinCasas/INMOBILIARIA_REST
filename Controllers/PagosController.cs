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
namespace INMOBILIARIA_REST.Controllers;

[Authorize]
public class PagosController : Controller
{

    public PagosController() { }

    public IActionResult Index()
    {
        try
        {
            RepositorioPago repositorioPago = new RepositorioPago();
            ViewBag.lista = repositorioPago.GetPagos();
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

    [HttpGet]
    public IActionResult Create(int? contratoId)
    {
        RepositorioPago repositorioPago = new RepositorioPago();
        ViewBag.listaContratos = repositorioPago.GetContratos();

        if (contratoId != null && contratoId > 0)
        {
            ViewBag.ContratoId = contratoId;
        }

        return View();
    }


    [HttpPost]
    public IActionResult Create(Pago pago)
    {
        try
        {
            RepositorioPago repositorioPago = new RepositorioPago();
            Contrato contrato = repositorioPago.ObtenerContratoPorId(pago.IdContrato);

            if (pago.Importe < contrato.MontoAlquilerMensual)
            {
                TempData["Error"] = "El importe ingresado no es suficiente para pagar el monto mensual del contrato";
                return RedirectToAction("Create");
            }

            int res = repositorioPago.Alta(pago);
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
            RepositorioPago repositorioPago = new RepositorioPago();
            Boolean res = repositorioPago.Baja(id);
            if (res == true)
            {
                return RedirectToAction("index");
            }
            else
            {
                TempData["Error"] = "Ocurrió un error al intentar eliminar el pago.";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo borrar el inmueble, para mas informacion contactarse con el administrador";
            return RedirectToAction("Index");
        }
    }


    [HttpGet]
    public IActionResult Update(int id)
    {
        try
        {
            RepositorioPago repositorioPago = new RepositorioPago();
            Pago pago = repositorioPago.ObtenerPorId(id);

            if (pago != null)
            {
                ViewBag.listaContratos = repositorioPago.GetContratos();
                ViewBag.pago = pago;
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
    public IActionResult UpdatePago(Pago pago)
    {
        try
        {
            RepositorioPago repositorioPagos = new RepositorioPago();
            Boolean res = repositorioPagos.Actualizar(pago);
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