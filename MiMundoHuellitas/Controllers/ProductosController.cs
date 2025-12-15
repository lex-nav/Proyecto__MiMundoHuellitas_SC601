using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();
        private const int TAM_PAGINA = 6;

        public ActionResult Index(int pagina = 1)
        {
            var query = db.MH_Productos_TB
                .Where(p => p.Activo && p.StockActual > 0)
                .OrderBy(p => p.NombreProducto);

            int totalRegistros = query.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / TAM_PAGINA);

            var productos = query
                .Skip((pagina - 1) * TAM_PAGINA)
                .Take(TAM_PAGINA)
                .Select(p => new ProductoItemViewModel
                {
                    IdProducto = p.IdProducto,
                    NombreProducto = p.NombreProducto,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    PrecioUnitario = p.PrecioUnitario,
                    StockActual = p.StockActual,
                    ImagenUrl = string.IsNullOrEmpty(p.ImagenUrl)
                        ? "/StaticFiles/img/producto-default.jpg"
                        : p.ImagenUrl
                })
                .ToList();

            return View(new ProductosListadoViewModel
            {
                Productos = productos,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas
            });
        }

        [HttpPost]
        public ActionResult AgregarCarrito(int idProducto)
        {
            var carrito = Session["CARRITO"] as System.Collections.Generic.List<int>
                          ?? new System.Collections.Generic.List<int>();

            carrito.Add(idProducto);
            Session["CARRITO"] = carrito;

            TempData["Ok"] = "Producto agregado al carrito 🐾";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
