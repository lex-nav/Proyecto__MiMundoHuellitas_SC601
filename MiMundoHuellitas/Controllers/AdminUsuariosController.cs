using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.EF;
using MiMundoHuellitas.DAL;
using MiMundoHuellitas.Models.ViewModels;
using MiMundoHuellitas.Helpers;

namespace MiMundoHuellitas.Controllers
{
    [Authorize] // luego puedes restringir por rol Admin
    public class AdminUsuariosController : Controller
    {
        private readonly MiMundoHuellitasEntities db =
            new MiMundoHuellitasEntities();

        // GET: /AdminUsuarios
        public ActionResult Index()
        {
            var usuarios = db.MH_Usuario_TB
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            return View(usuarios);
        }

        // GET: /AdminUsuarios/Edit/5
        public ActionResult Edit(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null)
                return HttpNotFound();

            return View(usuario);
        }

        // POST: /AdminUsuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MH_Usuario_TB model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = db.MH_Usuario_TB.Find(model.IdUsuario);
            if (usuario == null)
                return HttpNotFound();

            usuario.NombreCompleto = model.NombreCompleto;
            usuario.Correo = model.Correo;
            usuario.Telefono = model.Telefono;

            db.SaveChanges();

            TempData["Ok"] = "Usuario actualizado correctamente";
            return RedirectToAction("Index");
        }

        // GET: /AdminUsuarios/Delete/5
        public ActionResult Delete(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null)
                return HttpNotFound();

            return View(usuario);
        }

        // POST: /AdminUsuarios/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null)
                return HttpNotFound();

            db.MH_Usuario_TB.Remove(usuario);
            db.SaveChanges();

            TempData["Ok"] = "Usuario eliminado correctamente";
            return RedirectToAction("Index");
        }

        public ActionResult Historial(int id)
        {
            var repo = new HistorialClienteRepository();

            var model = new HistorialClienteViewModel
            {
                Cliente = repo.ObtenerCliente(id),
                Mascotas = repo.ObtenerMascotas(id),
                Citas = repo.ObtenerCitas(id),
                Facturas = repo.ObtenerFacturas(id)
            };

            if (model.Cliente == null)
                return HttpNotFound();

            return View(model);
        }
    }
}