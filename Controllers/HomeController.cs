using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TPRedes.Models;

namespace TPRedes.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        BD MiBd = new BD();
        string usuario = HttpContext.Session.GetString("NombreUsuario");   //Verifica que esta iniciada la sesion
        if (usuario != null)
        {
            ViewBag.Usuario = MiBd.DevUsuario(usuario);
            return View("PaginaPrincipal");
        }
        return View("IniciarSesion");
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Registrarse()
    {
        BD MiBd = new BD();
        string usuario = HttpContext.Session.GetString("NombreUsuario");
        if (usuario != null)
        {
            ViewBag.Usuario = MiBd.DevUsuario(usuario);
            return View("PaginaPrincipal");
        }
        return View();
    }

    [HttpPost]
    public IActionResult GuardarDatos(Usuario user)
    {
        BD MiBd = new BD();
        if (MiBd.VerificarNombreUsuario(user.NombreUsuario))
        {
            ViewBag.Repetido = true;
            return View("Registrarse", user);
        }
        else
        {
            MiBd.CargarUsuario(user);
        }
        return View("IniciarSesion");
    }

    [HttpPost]
    public IActionResult VerificarDatos(Usuario user)
    {
        BD MiBd = new BD();
        string usuario;
        if(MiBd.Registrarse(user.NombreUsuario, user.Contraseña))
        {
            HttpContext.Session.SetString("NombreUsuario", user.NombreUsuario);
            return RedirectToAction("Bienvenida"); 
        }
        ViewBag.MsjError = "Algo salió mal, verifique sus datos e intente nuevamente.";
        return View("IniciarSesion");
    }

    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return View("IniciarSesion");
    }

    public IActionResult Bienvenida()
    {
        BD MiBd = new BD();
        string usuario = HttpContext.Session.GetString("NombreUsuario");   //Verifica que esta iniciada la sesion
        if (usuario != null)
        {
            ViewBag.Usuario = MiBd.DevUsuario(usuario);
            return View("Red");
        }
        return View("IniciarSesion");
    }

    public IActionResult Red()
    {
        BD MiBd = new BD();
        string usuario = HttpContext.Session.GetString("NombreUsuario");
        if (usuario != null)
        {
            ViewBag.Usuario = MiBd.DevUsuario(usuario);
            return View();
        }
        return View("IniciarSesion");
    }

    [HttpGet]
    public IActionResult CrearPublicacion()
    {
        BD MiBd = new BD();
        string usuario = HttpContext.Session.GetString("NombreUsuario");
        if (usuario == null)
        {
            return View("IniciarSesion");
        }

        ViewBag.Usuario = MiBd.DevUsuario(usuario);
        return View();
    }

[HttpPost]
public IActionResult CrearPublicacion(Publicacion publicacion, IFormFile foto)
{
    BD MiBd = new BD();
    string usuario = HttpContext.Session.GetString("NombreUsuario");
    if (usuario == null)
    {
        return View("IniciarSesion");
    }

    ViewBag.Usuario = MiBd.DevUsuario(usuario);

    if (foto != null && foto.Length > 0)
    {
        string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", foto.FileName);

        using (var stream = new FileStream(ruta, FileMode.Create))
        {
            foto.CopyTo(stream);
        }

        publicacion.Imagen = foto.FileName;
    }

    if (MiBd.CargarPublicacion(publicacion))
    {
        return RedirectToAction("Red");
    }
    else
    {
        ViewBag.MensajeError = "Error al crear la publicación. Por favor, inténtelo de nuevo.";
        return View("CrearPublicacion");
    }
}

    [HttpGet]
    public IActionResult GetPublicaciones(int offset = 0, int limit = 10)
    {
        string usuarioActual = HttpContext.Session.GetString("NombreUsuario");
        if (usuarioActual == null)
        {
            return Json(new { ok = false, error = "No hay sesión activa" });
        }

        BD bd = new BD();
        var usuario = bd.DevUsuario(usuarioActual);
        if (usuario == null)
        {
            return Json(new { ok = false, error = "Usuario no encontrado" });
        }

        var publicaciones = bd.ObtenerPublicaciones(offset, limit);
        foreach (var p in publicaciones)
        {
            p.MeGusta = bd.UsuarioTieneLike(p.Id, usuario.Id);
        }

        return Json(new { ok = true, publicaciones });
    }

    [HttpPost]
    public IActionResult ToggleLike([FromBody] LikeRequest request)
    {
        string usuarioActual = HttpContext.Session.GetString("NombreUsuario");
        if (usuarioActual == null)
        {
            return Json(new { ok = false, error = "No hay sesión activa" });
        }

        BD bd = new BD();
        var usuario = bd.DevUsuario(usuarioActual);
        if (usuario == null)
        {
            return Json(new { ok = false, error = "Usuario no encontrado" });
        }

        if (!bd.PublicacionExiste(request.IdPublicacion))
        {
            return Json(new { ok = false, error = "La publicación no existe" });
        }

        var ok = bd.ToggleLike(request.IdPublicacion, usuario.Id);
        var cantidad = bd.ObtenerCantidadLikes(request.IdPublicacion);
        var meGusta = bd.UsuarioTieneLike(request.IdPublicacion, usuario.Id);

        return Json(new { ok = ok, meGusta, cantidadLikes = cantidad });
    }

    [HttpPost]
    public IActionResult AgregarComentario([FromBody] ComentarioRequest request)
    {
        string usuarioActual = HttpContext.Session.GetString("NombreUsuario");
        if (usuarioActual == null)
        {
            return Json(new { ok = false, error = "No hay sesión activa" });
        }

        BD bd = new BD();
        var usuario = bd.DevUsuario(usuarioActual);
        if (usuario == null)
        {
            return Json(new { ok = false, error = "Usuario no encontrado" });
        }

        if (!bd.PublicacionExiste(request.IdPublicacion))
        {
            return Json(new { ok = false, error = "La publicación no existe" });
        }

        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return Json(new { ok = false, error = "El comentario no puede estar vacío" });
        }

        var comentario = bd.AgregarComentario(request.IdPublicacion, usuario.Id, request.Texto);
        if (comentario == null)
        {
            return Json(new { ok = false, error = "No se pudo guardar el comentario" });
        }

        return Json(new
        {
            ok = true,
            comentario = new
            {
                id = comentario.Id,
                nombreUsuario = comentario.NombreUsuario,
                texto = comentario.Texto,
                fechaComentario = comentario.FechaComentario.ToString("yyyy-MM-dd HH:mm:ss")
            }
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class LikeRequest
{
    public int IdPublicacion { get; set; }
}

public class ComentarioRequest
{
    public int IdPublicacion { get; set; }
    public string Texto { get; set; }
}
