using System.Web.Mvc;
using MiMundoHuellitas.Helpers;
public class ServiciosController : Controller
{
    public ActionResult Index()
    {
        ViewBag.ActiveMenu = "Servicios";
        return View();
    }
}
