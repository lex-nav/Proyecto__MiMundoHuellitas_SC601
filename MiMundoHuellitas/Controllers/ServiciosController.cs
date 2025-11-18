using System.Web.Mvc;

public class ServiciosController : Controller
{
    public ActionResult Index()
    {
        ViewBag.ActiveMenu = "Servicios";
        return View();
    }
}
