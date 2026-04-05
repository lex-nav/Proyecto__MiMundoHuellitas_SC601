using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.Controllers
{
    [Authorize] // puedes restringir luego por rol Admin
    public class AdminProductosController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // GET: /AdminProductos
        public ActionResult Index()
        {
            var productos = db.MH_Productos_TB
                .OrderBy(p => p.NombreProducto)
                .ToList();

            return View(productos);
        }

        // GET: /AdminProductos/Edit/5
        public ActionResult Edit(int id)
        {
            var producto = db.MH_Productos_TB.Find(id);
            if (producto == null)
                return HttpNotFound();

            return View(producto);
        }

        // POST: /AdminProductos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MH_Productos_TB model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var producto = db.MH_Productos_TB.Find(model.IdProducto);
            if (producto == null)
                return HttpNotFound();

            producto.NombreProducto = model.NombreProducto;
            producto.Descripcion = model.Descripcion;
            producto.Categoria = model.Categoria;
            producto.PrecioUnitario = model.PrecioUnitario;
            producto.StockActual = model.StockActual;
            producto.ImagenUrl = model.ImagenUrl;
            producto.Activo = model.Activo;

            db.SaveChanges();

            TempData["Ok"] = "Producto actualizado correctamente";
            return RedirectToAction("Index");
        }

        // GET: /AdminProductos/Delete/5
        public ActionResult Delete(int id)
        {
            var producto = db.MH_Productos_TB.Find(id);
            if (producto == null)
                return HttpNotFound();

            return View(producto);
        }

        // POST: /AdminProductos/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int id)
        {
            var producto = db.MH_Productos_TB.Find(id);
            if (producto == null)
                return HttpNotFound();

            db.MH_Productos_TB.Remove(producto);
            db.SaveChanges();

            TempData["Ok"] = "Producto eliminado correctamente";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}