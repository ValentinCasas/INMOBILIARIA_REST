using MySqlConnector;
namespace INMOBILIARIA_REST.Models;

public class RepositorioInquilino
{

    string connectionString = "Server=localhost;User=root;Password=;Database=inmobiliariaCopia;SslMode=none";

    public RepositorioInquilino()
    {
    }

    public List<Multa> GetMultas()
    {
        List<Multa> multas = new List<Multa>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT Id, IdInquilino, Monto
                         FROM multa";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Multa multa = new Multa();
                        multa.Id = reader.GetInt32("Id");
                        multa.IdInquilino = reader.GetInt32("IdInquilino");
                        multa.Monto = reader.GetDecimal("Monto");
                        multas.Add(multa);
                    }
                }
                connection.Close();
            }
        }
        return multas;
    }

    public List<Inquilino> GetInquilinos()
    {
        List<Inquilino> inquilinos = new List<Inquilino>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var query = @"SELECT Id,Dni,Nombre,Apellido,Telefono,Email
            FROM inquilino";
            using (var command = new MySqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Inquilino inquilino = new Inquilino()
                        {
                            Id = reader.GetInt32(nameof(inquilino.Id)),
                            Dni = reader.GetInt64(nameof(inquilino.Dni)),
                            Nombre = reader.GetString(nameof(inquilino.Nombre)),
                            Apellido = reader.GetString(nameof(inquilino.Apellido)),
                            Telefono = reader.GetInt64(nameof(inquilino.Telefono)),
                            Email = reader.GetString(nameof(inquilino.Email)),
                        };
                        inquilinos.Add(inquilino);
                    }
                }
            }
            connection.Close();
        }
        return inquilinos;
    }

    public int Alta(Inquilino inquilino)
    {
        int res = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"INSERT INTO Inquilino (Nombre,Apellido,Dni,Telefono,Email) VALUES (@nombre,@apellido,@dni,@telefono,@email);
        SELECT LAST_INSERT_ID();";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nombre", inquilino.Nombre);
                command.Parameters.AddWithValue("@apellido", inquilino.Apellido);
                command.Parameters.AddWithValue("@dni", inquilino.Dni);
                command.Parameters.AddWithValue("@telefono", inquilino.Telefono);
                command.Parameters.AddWithValue("@email", inquilino.Email);
                connection.Open();
                res = Convert.ToInt32(command.ExecuteScalar());
                inquilino.Id = res;
                connection.Close();
            }
        }
        return res;
    }

    public Boolean Baja(int id)
    {
        Boolean res = false;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"DELETE FROM inquilino WHERE Id = @id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    res = true;
                }
            }
        }
        return res;
    }

    public Inquilino ObtenerPorId(int id)
    {
        Inquilino inquilino = null;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {

            string query = @"SELECT * FROM inquilino WHERE Id = @id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        inquilino = new Inquilino();
                        inquilino.Id = Convert.ToInt32(reader["id"]);
                        inquilino.Nombre = Convert.ToString(reader["nombre"]);
                        inquilino.Apellido = Convert.ToString(reader["apellido"]);
                        inquilino.Dni = Convert.ToInt64(reader["dni"]);
                        inquilino.Telefono = Convert.ToInt64(reader["telefono"]);
                        inquilino.Email = Convert.ToString(reader["email"]);
                        return inquilino;
                    }


                }
            }
        }
        return inquilino;
    }

    public Boolean Actualizar(Inquilino inquilino)
    {
        Boolean res = false;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"UPDATE inquilino SET Nombre = @nombre, Apellido = @apellido, Telefono = @telefono, Dni = @dni, Email = @email WHERE Id = @id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                if (inquilino.Nombre != null && inquilino.Apellido != null && inquilino.Dni > 0 && inquilino.Telefono > 0 && inquilino.Email != null)
                {
                    command.Parameters.AddWithValue("@id", inquilino.Id);
                    command.Parameters.AddWithValue("@nombre", inquilino.Nombre);
                    command.Parameters.AddWithValue("@apellido", inquilino.Apellido);
                    command.Parameters.AddWithValue("@dni", inquilino.Dni);
                    command.Parameters.AddWithValue("@telefono", inquilino.Telefono);
                    command.Parameters.AddWithValue("@email", inquilino.Email);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        res = true;
                    }
                }
            }
        }
        return res;
    }



}