using System.Web.Mvc;

public class HomeController : Controller
{
    public ActionResult Index()
    {
        ViewBag.ActiveMenu = "Inicio";
        return View("Index");
    }

    public ActionResult SobreNosotros()
    {
        ViewBag.ActiveMenu = "Nosotros";
        return View("SobreNosotros");
    }

    public ActionResult Equipo()
    {
        ViewBag.ActiveMenu = "Equipo";
        return View("Equipo");
    }

    public ActionResult Servicios()
    {
        ViewBag.ActiveMenu = "Servicios";
        return View("Servicios");
    }

    public ActionResult Productos()
    {
        ViewBag.ActiveMenu = "Productos";
        return View("Productos");
    }

    public ActionResult Blog()
    {
        ViewBag.ActiveMenu = "Blog";
        return View("Blog");
    }

    public ActionResult Galeria()
    {
        ViewBag.ActiveMenu = "Galeria";
        return View("Galeria");
    }

    public ActionResult Contacto()
    {
        ViewBag.ActiveMenu = "Contacto";
        return View("Contacto");
    }
}
