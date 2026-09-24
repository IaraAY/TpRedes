using Microsoft.Data.SqlClient;
using Dapper;

namespace TPRedes.Models;

public class BD
{
    private string _connectionString;

    public BD()
    {
        _connectionString = @"Server=localhost; DataBase=DBRedes;Integrated Security=True;TrustServerCertificate=True";
    }

    public void CargarUsuario(Usuario usuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            // CORREGIDO: @NombreUsuario coincide exactamente con la propiedad del objeto Usuario
            string query = @"INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido) 
                             VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido)";
            connection.Execute(query, usuario);
        }
    }

    public bool Registrarse(string nombreUsuario, string contraseña)
    {
        Usuario u;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios 
                             WHERE NombreUsuario = @NombreUsuario AND Contraseña = @Contraseña";
            u = connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = nombreUsuario, Contraseña = contraseña });
        }
        return u != null;
    }

    public bool VerificarNombreUsuario(string nombreUsuario)
    {
        Usuario u;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
            u = connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = nombreUsuario });
        }
        return u != null;
    }

    public Usuario DevUsuario(string nombreUsuario)
    {
        Usuario u = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
            u = connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = nombreUsuario });
        }
        return u;
    }
}