using INMOBILIARIA_REST.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

public class Pago
{

    public int Id { get; set; }

    public int IdContrato { get; set; }
    [ForeignKey(nameof(IdContrato))]
    public Contrato Contrato { get; set; }

    public int NumDePago { get; set; } = 0;
    public DateTime FechaDePago { get; set; }
    public decimal Importe { get; set; }

    public override string ToString()
    {
        return $"NumDePago: {NumDePago}, FechaDePago: {FechaDePago}, Importe: {Importe}";
    }


}