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
public class PropietarioController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment environment;

    public PropietarioController(DataContext context, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _context = context;
        _configuration = configuration;
        this.environment = environment;
    }


    // GET: Propietario/:id
    [HttpGet("{id:int:min(1)}")]
    public IActionResult GetPropietario(int id)
    {
        return Ok(_context.Propietario.Find(id));
    }



    [HttpGet("propietarios")]
    public IActionResult GetPropietarios()
    {
        return Ok(_context.Propietario.ToList());

    }

    //POST api/<controller>/login
    [HttpPost("loginn")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginView loginView)
    {
        try
        {
            var usuario = _context.Propietario.FirstOrDefault(x => x.Email == loginView.Usuario);
            if (usuario == null)
            {
                return NotFound();
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: loginView.Clave,
                salt: System.Text.Encoding.ASCII.GetBytes(_configuration["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 30000,
                numBytesRequested: 256 / 8));
            var p = await _context.Propietario.FirstOrDefaultAsync(x => x.Email == loginView.Usuario);
            if (p == null || p.Clave != hashed)
            {
                return BadRequest("Nombre de usuario o clave incorrecta");
            }
            else
            {
                var key = new SymmetricSecurityKey(
                    System.Text.Encoding.ASCII.GetBytes(_configuration["TokenAuthentication:SecretKey"]));
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, p.Email),
                    new Claim("FullName", p.Nombre + " " + p.Apellido),
                    new Claim(ClaimTypes.Role, "Propietario"),
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["TokenAuthentication:Issuer"],
                    audience: _configuration["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: credenciales
                );
                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    // GET api/propietario/data
    [HttpGet("data")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetUsuarioActual()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        if (identity != null)
        {
            var emailClaim = identity.FindFirst(ClaimTypes.Name);
            var fullNameClaim = identity.FindFirst("FullName");
            var roleClaim = identity.FindFirst(ClaimTypes.Role);

            var email = emailClaim?.Value;
            var fullName = fullNameClaim?.Value;
            var role = roleClaim?.Value;

            var propietario = _context.Propietario.FirstOrDefault(x => x.Email == email);

            if (propietario != null)
            {
                return Ok(propietario);
            }
        }

        return Unauthorized();
    }



    // GET api/propietario/imagen/{id}
    [HttpGet("imagen/{id:int:min(1)}")]
    public IActionResult ObtenerImagenPropietario(int id)
    {
        var propietario = _context.Propietario.FirstOrDefault(x => x.Id == id);
        if (propietario == null)
        {
            return NotFound();
        }

        var wwwPath = environment.WebRootPath;
        var ruta = Path.Combine(wwwPath, propietario.AvatarUrl.TrimStart('/').Replace('/', '\\'));

        if (ruta.StartsWith(Path.Combine(wwwPath, "Uploads")))
        {
            if (System.IO.File.Exists(ruta))
            {
                var fileBytes = System.IO.File.ReadAllBytes(ruta);
                return File(fileBytes, "image/jpeg");
            }
        }
        else
        {
            // Ruta por defecto para la imagen
            var defaultImagePath = Path.Combine(wwwPath, "imagenes", "avatar_por_defecto.jpg");
            if (System.IO.File.Exists(defaultImagePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(defaultImagePath);
                return File(fileBytes, "image/jpeg");
            }
        }

        return NotFound();
    }







}