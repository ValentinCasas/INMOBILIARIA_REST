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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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


    [HttpGet("propiedades-alquiladas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ObtenerPropiedadesAlquiladas()
    {
        var propietarioActual = ObtenerPropietarioLogueado();
        if (propietarioActual == null)
            return Unauthorized();

        var propiedadesAlquiladas = _context.Contrato
            .Where(contrato => contrato.Inmueble.Propietario.Id == propietarioActual.Id && contrato.Activo == true)
            .Select(contrato => contrato.Inmueble)
            .ToList();

        return Ok(propiedadesAlquiladas);
    }



    // Dado un inmueble retorna el contrato activo de dicho inmueble
    [HttpPost("contrato-vigente")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ObtenerContratoVigente([FromBody] Inmueble inmueble)
    {
        var propietarioActual = ObtenerPropietarioLogueado();
        if (propietarioActual == null)
        {
            return Unauthorized("No se encontró un propietario autenticado.");
        }

        var inmuebleEncontrado = _context.Inmueble.FirstOrDefault(i => i.Id == inmueble.Id && i.IdPropietario == propietarioActual.Id);
        if (inmuebleEncontrado == null)
        {
            return NotFound("No se encontró el inmueble especificado para el propietario actual.");
        }

        var contratoVigente = _context.Contrato
            .Include(c => c.Inquilino)
            .FirstOrDefault(contrato => contrato.IdInmueble == inmuebleEncontrado.Id && contrato.Activo);

        if (contratoVigente == null)
        {
            return NotFound("No se encontró un contrato vigente para el inmueble especificado.");
        }

        return Ok(contratoVigente);
    }


    //Dado un inmueble, retorna el inquilino del ultimo contrato activo de ese inmueble.
    [HttpPost("inquilino-ultimo-contrato")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ObtenerInquilinoUltimoContrato([FromBody] Inmueble inmueble)
    {
        var propietarioActual = ObtenerPropietarioLogueado();
        if (propietarioActual == null)
            return Unauthorized();

        var inmuebleEncontrado = _context.Inmueble
            .FirstOrDefault(i => i.Id == inmueble.Id && i.IdPropietario == propietarioActual.Id);

        if (inmuebleEncontrado == null)
            return NotFound();


        var contratoVigente = _context.Contrato
            .Include(c => c.Inquilino)
            .Where(contrato => contrato.IdInmueble == inmuebleEncontrado.Id && contrato.Activo)
            .OrderByDescending(contrato => contrato.FechaInicio)
            .FirstOrDefault();


        if (contratoVigente == null)
            return NotFound();


        return Ok(contratoVigente.Inquilino);
    }


    // Dado un Contrato, retorna los pagos de dicho contrato
    [HttpGet("pagos-contrato/{contratoId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ObtenerPagosContrato(int contratoId)
    {
        var propietarioActual = ObtenerPropietarioLogueado();
        if (propietarioActual == null)
            return Unauthorized();

        var contratoVer = _context.Contrato
            .Include(c => c.Inquilino)
            .Include(c => c.Inmueble)
                .ThenInclude(i => i.Propietario)
            .FirstOrDefault(c => c.Id == contratoId);

        if (contratoVer == null)
            return BadRequest("No se proporcionó un contrato válido.");

        var pagosContrato = _context.Pago
            .Include(p => p.Contrato)
                .ThenInclude(c => c.Inquilino)
            .Include(p => p.Contrato)
                .ThenInclude(c => c.Inmueble)
                    .ThenInclude(i => i.Propietario)
            .Where(pago => pago.IdContrato == contratoVer.Id)
            .Select(pago => new
            {
                pago.Id,
                pago.IdContrato,
                pago.Contrato,
                pago.NumDePago,
                pago.FechaDePago,
                pago.Importe
            })
            .ToList();


        return Ok(pagosContrato);
    }





// Actualizar Perfil
[HttpPost("actualizar-perfil")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public ActionResult<Propietario> ActualizarPerfil([FromBody] Propietario propietario)
{
    if (propietario == null)
    {
        return BadRequest("No se proporcionó un propietario válido.");
    }

    var propietarioExistente = _context.Propietario.FirstOrDefault(p => p.Id == propietario.Id);
    var emailExistente = _context.Propietario.Any(p => p.Id != propietario.Id && p.Email == propietario.Email);

    if (emailExistente)
    {
        return BadRequest("El email ya está en uso por otro propietario.");
    }

    if (propietarioExistente != null)
    {
        // Actualizar los campos del propietario existente
        propietarioExistente.Dni = propietario.Dni;
        propietarioExistente.Nombre = propietario.Nombre;
        propietarioExistente.Apellido = propietario.Apellido;
        propietarioExistente.Telefono = propietario.Telefono;
        propietarioExistente.Email = propietario.Email;

        if (!string.IsNullOrEmpty(propietario.Clave))
        {
            // Validar la nueva contraseña
            if (propietario.Clave.Length < 8 || !propietario.Clave.Any(char.IsUpper))
            {
                return BadRequest("La contraseña debe tener al menos 8 caracteres y contener al menos una letra mayúscula.");
            }

            // Hashear la contraseña
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: propietario.Clave,
                salt: System.Text.Encoding.ASCII.GetBytes(_configuration["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 30000,
                numBytesRequested: 256 / 8));

            propietarioExistente.Clave = hashedPassword;
        }

        propietarioExistente.AvatarUrl = propietario.AvatarUrl;

        // Guardar los cambios en la base de datos
        _context.SaveChanges();

        return Ok(propietarioExistente);
    }

    return NotFound("No se encontró el propietario especificado.");
}



    //ActualizarInmueble
    [HttpPost("actualizar-inmueble")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ActualizarInmueble([FromBody] Inmueble inmueble)
    {
        if (inmueble == null)
        {
            return BadRequest("No se proporcionó un inmueble válido.");
        }

        var inmuebleExistente = _context.Inmueble.Include(i => i.Propietario).FirstOrDefault(i => i.Id == inmueble.Id);
        if (inmuebleExistente != null)
        {
            // Actualizar los campos del inmueble existente con los valores proporcionados
            inmuebleExistente.IdPropietario = inmueble.IdPropietario;
            inmuebleExistente.Direccion = inmueble.Direccion;
            inmuebleExistente.Uso = inmueble.Uso;
            inmuebleExistente.Tipo = inmueble.Tipo;
            inmuebleExistente.CantidadAmbientes = inmueble.CantidadAmbientes;
            inmuebleExistente.Coordenadas = inmueble.Coordenadas;
            inmuebleExistente.PrecioInmueble = inmueble.PrecioInmueble;
            inmuebleExistente.Estado = inmueble.Estado;

            // Guardar los cambios en la base de datos
            _context.SaveChanges();

            return Ok(inmuebleExistente);
        }

        return NotFound("No se encontró el inmueble especificado.");
    }




    private Propietario ObtenerPropietarioLogueado()
    {
        var email = User.Identity.Name;
        var propietario = _context.Propietario.FirstOrDefault(p => p.Email == email);
        return propietario;
    }



}