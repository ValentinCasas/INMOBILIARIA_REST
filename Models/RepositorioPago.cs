using MySqlConnector;
namespace INMOBILIARIA_REST.Models;

public class RepositorioPago
{

    string connectionString = "Server=localhost;User=root;Password=;Database=inmobiliariaCopia;SslMode=none";

    public RepositorioPago() { }

    public List<Pago> GetPagos()
    {
        List<Pago> pagos = new List<Pago>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var query = @"SELECT i.Id, i.NumDePago, i.FechaDePago, i.Importe, i.IdContrato,
                        c.Id, c.FechaInicio, c.Fechafinalizacion, c.MontoAlquilerMensual, c.Activo, c.IdInquilino
                        FROM pago i
                        JOIN contrato c ON i.IdContrato = c.Id
                        ";

            using (var command = new MySqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pago pago = new Pago()
                        {
                            Id = reader.GetInt32(nameof(pago.Id)),
                            NumDePago = reader.GetInt32(nameof(pago.NumDePago)),
                            FechaDePago = reader.GetDateTime(nameof(pago.FechaDePago)),
                            Importe = reader.GetDecimal(nameof(pago.Importe)),

                            Contrato = new Contrato()
                            {
                                Id = reader.GetInt32(nameof(Contrato.Id)),
                                FechaInicio = reader.GetDateTime(nameof(Contrato.FechaInicio)),
                                FechaFinalizacion = reader.GetDateTime(nameof(Contrato.FechaFinalizacion)),
                                MontoAlquilerMensual = reader.GetDecimal(nameof(Contrato.MontoAlquilerMensual)),
                                Activo = reader.GetBoolean(nameof(Contrato.Activo)),
                                IdInquilino = reader.GetInt32(nameof(Contrato.IdInquilino)),
                            },
                        };
                        pagos.Add(pago);
                    }
                }
            }
        }
        return pagos;
    }

    public Contrato ObtenerContratoPorId(int id)
    {
        Contrato contrato = null;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {

            string query = @"SELECT * FROM contrato WHERE Id = @id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contrato = new Contrato();
                        contrato.Id = Convert.ToInt32(reader["id"]);
                        contrato.IdInquilino = Convert.ToInt32(reader["IdInquilino"]);
                        contrato.IdInmueble = Convert.ToInt32(reader["IdInmueble"]);
                        contrato.FechaInicio = Convert.ToDateTime(reader["FechaInicio"]);
                        contrato.FechaFinalizacion = Convert.ToDateTime(reader["FechaFinalizacion"]);
                        contrato.MontoAlquilerMensual = Convert.ToDecimal(reader["MontoAlquilerMensual"]);
                        contrato.Activo = Convert.ToBoolean(reader["Activo"]);

                        return contrato;
                    }


                }
            }
        }
        return contrato;
    }
    public List<Contrato> GetContratos()
    {
        List<Contrato> contratos = new List<Contrato>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var query = @"SELECT Id,IdInquilino,FechaInicio,FechaFinalizacion,MontoAlquilerMensual,Activo
            FROM contrato";
            using (var command = new MySqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contrato contrato = new Contrato()
                        {
                            Id = reader.GetInt32(nameof(contrato.Id)),
                            IdInquilino = reader.GetInt32(nameof(contrato.IdInquilino)),
                            FechaInicio = reader.GetDateTime(nameof(contrato.FechaInicio)),
                            FechaFinalizacion = reader.GetDateTime(nameof(contrato.FechaFinalizacion)),
                            MontoAlquilerMensual = reader.GetDecimal(nameof(contrato.MontoAlquilerMensual)),
                            Activo = reader.GetBoolean(nameof(contrato.Activo)),
                        };
                        contratos.Add(contrato);
                    }
                }
            }
            connection.Close();
        }
        return contratos;
    }

    public int Alta(Pago pago)
    {

        List<Pago> listaPagosPorIdContrato = ObtenerPagosPorIdContrato(pago.IdContrato);

        int res = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"INSERT INTO pago (NumDePago,FechaDePago,Importe,IdContrato)
             VALUES (@numDePago,@fechaDePago,@importe,@idContrato);
        SELECT LAST_INSERT_ID();";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@numDePago", listaPagosPorIdContrato.Count() + 1);
                command.Parameters.AddWithValue("@fechaDePago", pago.FechaDePago);
                command.Parameters.AddWithValue("@importe", pago.Importe);
                command.Parameters.AddWithValue("@idContrato", pago.IdContrato);

                connection.Open();
                res = Convert.ToInt32(command.ExecuteScalar());
                pago.Id = res;
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
            string query = @"DELETE FROM pago WHERE Id = @id";
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

    public Pago ObtenerPorId(int id)
    {
        Pago pago = null;

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var query = @"SELECT i.Id, i.NumDePago, i.FechaDePago, i.Importe, i.IdContrato,
                    c.Id, c.FechaInicio, c.Fechafinalizacion, c.MontoAlquilerMensual, c.Activo, c.IdInquilino
                    FROM pago i
                    JOIN contrato c ON i.IdContrato = c.Id
                    WHERE i.Id = @id";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pago = new Pago()
                        {
                            Id = reader.GetInt32(nameof(Pago.Id)),
                            NumDePago = reader.GetInt32(nameof(Pago.NumDePago)),
                            FechaDePago = reader.GetDateTime(nameof(Pago.FechaDePago)),
                            Importe = reader.GetDecimal(nameof(Pago.Importe)),

                            Contrato = new Contrato()
                            {
                                Id = reader.GetInt32(nameof(Contrato.Id)),
                                FechaInicio = reader.GetDateTime(nameof(Contrato.FechaInicio)),
                                FechaFinalizacion = reader.GetDateTime(nameof(Contrato.FechaFinalizacion)),
                                MontoAlquilerMensual = reader.GetDecimal(nameof(Contrato.MontoAlquilerMensual)),
                                Activo = reader.GetBoolean(nameof(Contrato.Activo)),
                                IdInquilino = reader.GetInt32(nameof(Contrato.IdInquilino)),
                            },
                        };
                    }
                }
            }
        }

        return pago;
    }

    public List<Pago> ObtenerPagosPorIdContrato(int idContrato)
    {
        List<Pago> pagos = new List<Pago>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var query = @"SELECT p.Id, p.NumDePago, p.FechaDePago, p.Importe, p.IdContrato,
                    c.Id, c.FechaInicio, c.FechaFinalizacion, c.MontoAlquilerMensual, c.Activo, c.IdInquilino
                    FROM pago p
                    JOIN contrato c ON p.IdContrato = c.Id
                    WHERE p.IdContrato = @idContrato";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idContrato", idContrato);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pago pago = new Pago()
                        {
                            Id = reader.GetInt32(nameof(Pago.Id)),
                            NumDePago = reader.GetInt32(nameof(Pago.NumDePago)),
                            FechaDePago = reader.GetDateTime(nameof(Pago.FechaDePago)),
                            Importe = reader.GetDecimal(nameof(Pago.Importe)),

                            Contrato = new Contrato()
                            {
                                Id = reader.GetInt32(nameof(Contrato.Id)),
                                FechaInicio = reader.GetDateTime(nameof(Contrato.FechaInicio)),
                                FechaFinalizacion = reader.GetDateTime(nameof(Contrato.FechaFinalizacion)),
                                MontoAlquilerMensual = reader.GetDecimal(nameof(Contrato.MontoAlquilerMensual)),
                                Activo = reader.GetBoolean(nameof(Contrato.Activo)),
                                IdInquilino = reader.GetInt32(nameof(Contrato.IdInquilino)),
                            },
                        };
                        pagos.Add(pago);
                    }
                }
            }
        }

        return pagos;
    }


    public Boolean Actualizar(Pago pago)
    {
        Boolean res = false;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"UPDATE pago SET IdContrato = @idContrato, Importe = @importe, FechaDePago = @fechaDePago WHERE Id = @id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
     
                    command.Parameters.AddWithValue("@id", pago.Id);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@fechaDePago", pago.FechaDePago);
                    command.Parameters.AddWithValue("@idContrato", pago.IdContrato);

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


}