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

    //Hacer una función que se pueda conectar al Java para que cuando se seleccione un país en el dropdown de países, se obtengan las provincias de ese país y se muestren en el dropdown de provincias. La función tiene que recibir como parámetro el país seleccionado y devolver un JSON con las provincias de ese país.

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
