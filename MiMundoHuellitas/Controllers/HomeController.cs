using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.Helpers;

namespace MiMundoHuellitas.Controllers
{
    public class HomeController : Controller
    {
        private readonly MiMundoHuellitasEntities db = new MiMundoHuellitasEntities();

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

        // ✅ AHORA: Productos lee tabla MH_Productos_TB y pagina de 6 en 6
        public ActionResult Productos(int page = 1)
        {
            ViewBag.ActiveMenu = "Productos";

            const int pageSize = 6;
            if (page < 1) page = 1;

            var query = db.MH_Productos_TB
                .Where(p => p.Activo && p.StockActual > 0)
                .OrderBy(p => p.IdProducto);

            int total = query.Count();
            int totalPages = (int)Math.Ceiling((double)total / pageSize);
            if (totalPages <= 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var productos = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new HomeProductosViewModel
            {
                Productos = productos,
                CurrentPage = page,
                TotalPages = totalPages
            };

            // Vista: Views/Home/Productos.cshtml
            return View("Productos", model);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
