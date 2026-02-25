using MiMundoHuellitas.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsuariosController : Controller
    {
        private readonly MiMundoHuellitasEntities db =   new MiMundoHuellitasEntities();

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

            // Marcar salida
            usuario.FechaSalida = DateTime.Now;

            // Anonimizar datos
            usuario.NombreCompleto = "Ex-Empleado #" + usuario.IdUsuario;
            usuario.Correo = $"anon{usuario.IdUsuario}@system.local";
            usuario.Telefono = null;
            usuario.Activo = false;

            db.SaveChanges();

            TempData["Ok"] = "Usuario desactivado y anonimizado correctamente";
            return RedirectToAction("Index");
        }
    }
}
