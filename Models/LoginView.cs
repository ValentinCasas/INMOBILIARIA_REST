using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace INMOBILIARIA_REST.Models
{
public class LoginView
{
    [DataType(DataType.EmailAddress)]
    public string Usuario { get; set; }

    [DataType(DataType.Password)]
    public string Clave { get; set; }
}

}