using INMOBILIARIA_REST.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace INMOBILIARIA_REST.Models{

public class Inmueble
{

    public int Id { get; set; }

    public int IdPropietario { get; set; }
    [ForeignKey(nameof(IdPropietario))]
    public Propietario Propietario { get; set; }

    public string? Direccion { get; set; }
    public string? Uso { get; set; } //comercial o residencial
    public string? Tipo { get; set; } //local, deposito, casa, departamento, etc
    public int CantidadAmbientes { get; set; }
    public string? Coordenadas { get; set; }
    public decimal PrecioInmueble { get; set; }
    public string? Estado { get; set; } //disponible no disponible

    public string AvatarUrl { get; set; }
    [NotMapped]
    public IFormFile? AvatarFile { get; set; }

    public override string ToString()
    {
        return 
               $"{Propietario.Nombre} |  " +
               $" {Direccion} | " +
               $" {Uso} | " +
               $" {Tipo} | " +
               $" ambientes: {CantidadAmbientes} | " +
               $" {Coordenadas} | " +
               $" $ {PrecioInmueble} | " +
               $" Estado: {Estado}";
    }


}

}