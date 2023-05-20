using INMOBILIARIA_REST.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
public class Contrato
{

    public int Id { get; set; }

    public int IdInquilino { get; set; }
    [ForeignKey(nameof(IdInquilino))]
    public Inquilino Inquilino { get; set; }

    public int IdInmueble { get; set; }
    [ForeignKey(nameof(IdInmueble))]
    public Inmueble Inmueble { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFinalizacion { get; set; }
    public decimal MontoAlquilerMensual { get; set; }
    public Boolean Activo { get; set; }


    public override string ToString()
    {
        return $"inicio: {FechaInicio} | fin: {FechaFinalizacion}| $ {MontoAlquilerMensual}|Activo: {(Activo ? "si" : "no")}";
    }

}

