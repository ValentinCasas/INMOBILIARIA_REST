using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;

namespace INMOBILIARIA_REST.Models;

public class RepositorioUsuario
{
    string connectionString = "Server=localhost;User=root;Password=;Database=inmobiliariaCopia;SslMode=none";

    public RepositorioUsuario()
    {
    }

    public bool Baja(int idUsuario)
    {
        bool res = false;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"DELETE FROM usuario WHERE Id = @idUsuario";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idUsuario", idUsuario);
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

    public int Alta(Usuario usuario)
    {
        int res = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"INSERT INTO usuario 
					(Nombre, Apellido, AvatarUrl, Edad, Dni, Email, Clave, Rol) 
					VALUES (@nombre, @apellido, @avatarUrl, @edad, @dni, @email, @clave, @rol);
					SELECT LAST_INSERT_ID();";



            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@edad", usuario.Edad);
                command.Parameters.AddWithValue("@dni", usuario.Dni);
                command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                /* if (String.IsNullOrEmpty(usuario.AvatarUrl))
                    command.Parameters.AddWithValue("@avatarUrl", DBNull.Value);
                else */
                command.Parameters.AddWithValue("@avatarUrl", usuario.AvatarUrl);
                command.Parameters.AddWithValue("@email", usuario.Email);
                command.Parameters.AddWithValue("@clave", usuario.Clave);
                command.Parameters.AddWithValue("@rol", usuario.Rol);
                connection.Open();
                res = Convert.ToInt32(command.ExecuteScalar());
                usuario.Id = res;
                connection.Close();
            }
        }
        return res;
    }

    public Boolean Actualizar(Usuario usuario)
    {
        Boolean res = false;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"UPDATE usuario 
					SET Nombre=@nombre, Apellido=@apellido, Dni=@dni, Edad=@edad, AvatarUrl=@avatarUrl, Email=@email, Clave=@clave, Rol=@rol
					WHERE Id = @id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                command.Parameters.AddWithValue("@avatarUrl", usuario.AvatarUrl);
                command.Parameters.AddWithValue("@edad", usuario.Edad);
                command.Parameters.AddWithValue("@dni", usuario.Dni);
                command.Parameters.AddWithValue("@email", usuario.Email);
                command.Parameters.AddWithValue("@clave", usuario.Clave);
                command.Parameters.AddWithValue("@rol", usuario.Rol);
                command.Parameters.AddWithValue("@id", usuario.Id);
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

    public List<Usuario> GetUsuarios()
    {
        List<Usuario> usuarios = new List<Usuario>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var query = @"SELECT Id,Dni,Nombre,Apellido,Edad,Email, Clave, AvatarUrl,Rol
            FROM usuario";
            using (var command = new MySqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Usuario usuario = new Usuario()
                        {
                            Id = reader.GetInt32(nameof(usuario.Id)),
                            Dni = reader.GetInt64(nameof(usuario.Dni)),
                            Nombre = reader.GetString(nameof(usuario.Nombre)),
                            Apellido = reader.GetString(nameof(usuario.Apellido)),
                            Edad = reader.GetInt32(nameof(usuario.Edad)),
                            Email = reader.GetString(nameof(usuario.Email)),
                            Clave = reader.GetString(nameof(usuario.Clave)),
                            AvatarUrl = reader.GetString(nameof(usuario.AvatarUrl)),
                            Rol = reader.GetInt32(nameof(usuario.Rol)),
                        };
                        usuarios.Add(usuario);
                    }
                }
            }
            connection.Close();
        }
        return usuarios;
    }

    public List<Usuario> BuscarUsuariosQuery(string query)
    {
        List<Usuario> usuarios = new List<Usuario>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            var searchQuery = $@"SELECT Id,Dni,Nombre,Apellido,Edad,Email, Clave, AvatarUrl,Rol
        FROM usuario
        WHERE Nombre LIKE '%{query}%' OR Apellido LIKE '%{query}%'";
            using (var command = new MySqlCommand(searchQuery, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Usuario usuario = new Usuario()
                        {
                            Id = reader.GetInt32(nameof(usuario.Id)),
                            Dni = reader.GetInt64(nameof(usuario.Dni)),
                            Nombre = reader.GetString(nameof(usuario.Nombre)),
                            Apellido = reader.GetString(nameof(usuario.Apellido)),
                            Edad = reader.GetInt32(nameof(usuario.Edad)),
                            Email = reader.GetString(nameof(usuario.Email)),
                            Clave = reader.GetString(nameof(usuario.Clave)),
                            AvatarUrl = reader.GetString(nameof(usuario.AvatarUrl)),
                            Rol = reader.GetInt32(nameof(usuario.Rol)),
                        };
                        usuarios.Add(usuario);
                    }
                }
            }
            connection.Close();
        }
        return usuarios;
    }


    public Usuario ObtenerPorEmail(string email)
    {
        Usuario? usuario = null;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT
					Id, Nombre, Apellido, AvatarUrl, Dni, Edad, Email, Clave, Rol FROM usuario
					WHERE Email=@email";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@email", email);
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario();
                        usuario.Id = reader.GetInt32("Id");
                        usuario.Nombre = reader.GetString("Nombre");
                        usuario.Apellido = reader.GetString("Apellido");
                        usuario.Dni = reader.GetInt64("Dni");
                        usuario.Edad = reader.GetInt32("Edad");
                        usuario.AvatarUrl = reader.GetString("AvatarUrl");
                        usuario.Email = reader.GetString("Email");
                        usuario.Clave = reader.GetString("Clave");
                        usuario.Rol = reader.GetInt32("Rol");

                        return usuario;
                    }
                }

            }
        }
        return usuario;
    }

    public List<Usuario> ObtenerPorEmailList(string email)
    {
        List<Usuario> usuarios = new List<Usuario>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT
				Id, Nombre, Apellido, AvatarUrl, Dni, Edad, Email, Clave, Rol FROM usuario
				WHERE Email=@email";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@email", email);
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Usuario usuario = new Usuario();
                        usuario.Id = reader.GetInt32("Id");
                        usuario.Nombre = reader.GetString("Nombre");
                        usuario.Apellido = reader.GetString("Apellido");
                        usuario.Dni = reader.GetInt64("Dni");
                        usuario.Edad = reader.GetInt32("Edad");
                        usuario.AvatarUrl = reader.GetString("AvatarUrl");
                        usuario.Email = reader.GetString("Email");
                        usuario.Clave = reader.GetString("Clave");
                        usuario.Rol = reader.GetInt32("Rol");

                        usuarios.Add(usuario);
                    }
                }

            }
        }
        return usuarios;
    }

    public Usuario ObtenerPorId(int id)
    {
        Usuario? usuario = null;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT
					Id, Nombre, Apellido, AvatarUrl, Dni, Edad, Email, Clave, Rol FROM usuario
					WHERE Id=@id";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario();
                        usuario.Id = reader.GetInt32("Id");
                        usuario.Nombre = reader.GetString("Nombre");
                        usuario.Apellido = reader.GetString("Apellido");
                        usuario.Dni = reader.GetInt64("Dni");
                        usuario.Edad = reader.GetInt32("Edad");
                        usuario.AvatarUrl = reader.GetString("AvatarUrl");
                        usuario.Email = reader.GetString("Email");
                        usuario.Clave = reader.GetString("Clave");
                        usuario.Rol = reader.GetInt32("Rol");

                        return usuario;
                    }
                }

            }
        }
        return usuario;
    }

}
