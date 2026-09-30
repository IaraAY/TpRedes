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
    // Agrega un método para guardar una publicación, hacer verificación de: el titulo y la descripcion tienen que tener texto ingresado.
    public bool CargarPublicacion(Publicacion publicacion)
    {
        if (string.IsNullOrWhiteSpace(publicacion.Titulo) || string.IsNullOrWhiteSpace(publicacion.Descripcion))
        {
            return false;
        }
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Publicaciones (IdUsuario, Titulo, Descripcion, FechaPublicacion, Imagen) 
                             VALUES (@IdUsuario, @Titulo, @Descripcion, @FechaPublicacion, @Imagen)";
            connection.Execute(query, publicacion);
        }
        return true;
    }

    public List<Publicacion> ObtenerPublicaciones(int offset, int limit)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT p.Id, p.IdUsuario, p.Titulo, p.Descripcion, p.FechaPublicacion, p.Imagen,
                       u.NombreUsuario,
                       (SELECT COUNT(*) FROM PublicacionesMeGusta pmg WHERE pmg.IdPublicación = p.Id) AS CantidadLikes
                FROM Publicaciones p
                INNER JOIN Usuarios u ON u.Id = p.IdUsuario
                ORDER BY p.FechaPublicacion DESC
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

            var publicaciones = connection.Query<Publicacion>(query, new { Offset = offset, Limit = limit }).ToList();

            foreach (var publicacion in publicaciones)
            {
                publicacion.Comentarios = ObtenerComentarios(publicacion.Id);
            }

            return publicaciones;
        }
    }

    public int ObtenerCantidadLikes(int idPublicacion)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT COUNT(*) FROM PublicacionesMeGusta WHERE IdPublicación = @IdPublicacion";
            return connection.ExecuteScalar<int>(query, new { IdPublicacion = idPublicacion });
        }
    }

    public bool UsuarioTieneLike(int idPublicacion, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT COUNT(*) FROM PublicacionesMeGusta WHERE IdPublicación = @IdPublicacion AND IdUsuario = @IdUsuario";
            return connection.ExecuteScalar<int>(query, new { IdPublicacion = idPublicacion, IdUsuario = idUsuario }) > 0;
        }
    }

    public bool PublicacionExiste(int idPublicacion)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT COUNT(*) FROM Publicaciones WHERE Id = @IdPublicacion";
            return connection.ExecuteScalar<int>(query, new { IdPublicacion = idPublicacion }) > 0;
        }
    }

    public bool ToggleLike(int idPublicacion, int idUsuario)
    {
        if (!PublicacionExiste(idPublicacion))
            return false;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            var existe = connection.QueryFirstOrDefault<int>(@"SELECT Id FROM PublicacionesMeGusta WHERE IdPublicación = @IdPublicacion AND IdUsuario = @IdUsuario",
                new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });

            if (existe > 0)
            {
                connection.Execute(@"DELETE FROM PublicacionesMeGusta WHERE IdPublicación = @IdPublicacion AND IdUsuario = @IdUsuario",
                    new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });
                return true;
            }

            connection.Execute(@"INSERT INTO PublicacionesMeGusta (IdPublicación, IdUsuario) VALUES (@IdPublicacion, @IdUsuario)",
                new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });
            return true;
        }
    }

    public List<Comentario> ObtenerComentarios(int idPublicacion)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT c.Id, c.IdPublicacion, c.IdUsuarioComenta, c.Texto, c.FechaComentario, u.NombreUsuario
                FROM Comentarios c
                INNER JOIN Usuarios u ON u.Id = c.IdUsuarioComenta
                WHERE c.IdPublicacion = @IdPublicacion
                ORDER BY c.FechaComentario ASC";

            return connection.Query<Comentario>(query, new { IdPublicacion = idPublicacion }).ToList();
        }
    }

    public Comentario AgregarComentario(int idPublicacion, int idUsuario, string texto)
    {
        if (!PublicacionExiste(idPublicacion))
            return null;

        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var comentario = new Comentario
        {
            IdPublicacion = idPublicacion,
            IdUsuarioComenta = idUsuario,
            Texto = texto.Trim(),
            FechaComentario = DateTime.Now
        };

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                INSERT INTO Comentarios (IdPublicacion, IdUsuarioComenta, Texto, FechaComentario)
                VALUES (@IdPublicacion, @IdUsuarioComenta, @Texto, @FechaComentario);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            comentario.Id = connection.QuerySingle<int>(query, comentario);
        }

        var usuario = DevUsuarioPorId(idUsuario);
        comentario.NombreUsuario = usuario?.NombreUsuario;
        return comentario;
    }

    public Usuario DevUsuarioPorId(int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios WHERE Id = @IdUsuario";
            return connection.QueryFirstOrDefault<Usuario>(query, new { IdUsuario = idUsuario });
        }
    }
}