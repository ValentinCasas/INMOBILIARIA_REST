using INMOBILIARIA_REST.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
public class Multa
{

    public int Id { get; set; }

    public int IdInquilino { get; set; }
    [ForeignKey(nameof(IdInquilino))]
    public Inquilino Inquilino { get; set; }

    public decimal Monto { get; set; }

}

