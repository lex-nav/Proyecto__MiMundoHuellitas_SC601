using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.EF;
using System.Data.Entity;


namespace MiMundoHuellitas.Controllers
{
    [Authorize] // luego puedes restringir por rol Admin
    public class AdminUsuariosController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db =
            new BD_MiMundoHuellitasEntities();

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
            var usuario = db.MH_Usuario_TB
        .Include(u => u.MH_Tipo_Usuario_TB)
        .Include(u => u.MH_Jornada_TB)
        .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null) return HttpNotFound();

            // Jornadas activas para dropdown
            ViewBag.Jornadas = db.MH_Jornada_TB
                .Where(j => j.Activo)
                .OrderBy(j => j.Nombre)
                .Select(j => new SelectListItem
                {
                    Value = j.IdJornada.ToString(),
                    Text = j.Nombre + " (" + j.HoraEntrada + " - " + j.HoraSalida + ")",
                    Selected = (usuario.IdJornada != null && usuario.IdJornada == j.IdJornada)
                })
                .ToList();

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
            if (usuario == null) return HttpNotFound();

            usuario.NombreCompleto = model.NombreCompleto;
            usuario.Correo = model.Correo;
            usuario.Telefono = model.Telefono;

            // ✅ NUEVO: asignación de jornada
            usuario.IdJornada = model.IdJornada;

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
    }
}
