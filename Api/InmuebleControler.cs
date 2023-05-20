using System;
using System.Collections.Generic;
using INMOBILIARIA_REST.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace INMOBILIARIA_REST.Api;

[ApiController]
[Route("api/[controller]")]
public class InmuebleController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;

    public InmuebleController(DataContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


    [HttpGet("propiedades")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ObtenerPropiedades()
    {
        var propietarioActual = ObtenerPropietarioLogueado();
        return propietarioActual == null
                                    ? Unauthorized()
                                    : Ok(_context.Inmueble
                                               .Where(inmueble => inmueble.IdPropietario == propietarioActual.Id)
                                               .ToList());
    }


    private Propietario ObtenerPropietarioLogueado()
    {
        var email = User.Identity.Name;
        var propietario = _context.Propietario.FirstOrDefault(p => p.Email == email);
        return propietario;
    }



}