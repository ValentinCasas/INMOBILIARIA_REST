using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace INMOBILIARIA_REST.Models
{

    public class Propietario
    {
        public int Id { get; set; }
        [Required]
        public long Dni { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellido { get; set; }
        [Required]
        public long Telefono { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Clave { get; set; }

        public string AvatarUrl { get; set; }
        [NotMapped]
        public IFormFile? AvatarFile { get; set; }

        [NotMapped]
        public string? ClaveAntigua { get; set; }

        [NotMapped]
        public string? NuevaClave { get; set; }

        [NotMapped]
        public string? ConfirmarClave { get; set; }


        public override string ToString()
        {
            return $"Dni: {Dni}, Nombre: {Nombre}, Apellido: {Apellido}, Teléfono: {Telefono}, Email: {Email}";
        }




    }

}