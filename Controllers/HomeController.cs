using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using INMOBILIARIA_REST.Models;

namespace INMOBILIARIA_REST.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration config;

    public HomeController(IConfiguration config, ILogger<HomeController> logger)
    {
        this.config = config;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public ActionResult Restringido()
    {
        return View();
    }


}
