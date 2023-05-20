using INMOBILIARIA_REST.Models;

public class Inquilino
{
    public int Id { get; set; }
    public long Dni { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public long Telefono { get; set; }
    public string Email { get; set; }

    public override string ToString()
    {
        return $"Nombre: {Nombre}, Apellido: {Apellido}, Dni: {Dni}, Telefono: {Telefono}, Email: {Email}";
    }


}

