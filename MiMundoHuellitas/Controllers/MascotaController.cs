using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    public class MascotaController : Controller
    {
        [HttpGet]
        public ActionResult VerMascotas()
        {
                return View();
        }

        [HttpGet]
        public ActionResult AgregarMascota()
        {
            return View();
        }
    }
}