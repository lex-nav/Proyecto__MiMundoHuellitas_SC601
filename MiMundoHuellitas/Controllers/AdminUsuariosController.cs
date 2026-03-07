using MiMundoHuellitas.EF;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsuariosController : Controller
    {
        private readonly MiMundoHuellitasEntities db = new MiMundoHuellitasEntities();

        public ActionResult Index()
        {
            var usuarios = db.MH_Usuario_TB
                .Include(u => u.MH_Tipo_Usuario_TB)
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            return View(usuarios);
        }

        public ActionResult Edit(int id)
        {
            var usuario = db.MH_Usuario_TB
                .Include(u => u.MH_Tipo_Usuario_TB)
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null) return HttpNotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MH_Usuario_TB model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = db.MH_Usuario_TB.FirstOrDefault(u => u.IdUsuario == model.IdUsuario);
            if (usuario == null) return HttpNotFound();

            string oldNombre = (usuario.NombreCompleto ?? "").Trim();
            string oldCorreo = (usuario.Correo ?? "").Trim();
            string oldTelefono = (usuario.Telefono ?? "").Trim();

            string newNombre = (model.NombreCompleto ?? "").Trim();
            string newCorreo = (model.Correo ?? "").Trim();
            string newTelefono = (model.Telefono ?? "").Trim();

            string adminCorreo = (User?.Identity?.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(adminCorreo))
                adminCorreo = "admin";

            usuario.NombreCompleto = newNombre;
            usuario.Correo = newCorreo;
            usuario.Telefono = newTelefono;
            usuario.Activo = model.Activo;
            usuario.IdTipoUsuario = model.IdTipoUsuario;
            usuario.IdDireccion = model.IdDireccion;

            db.SaveChanges();

            if (!StringEquals(oldNombre, newNombre))
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "NombreCompleto", oldNombre, newNombre, "UPDATE", adminCorreo);

            if (!StringEquals(oldCorreo, newCorreo))
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "Correo", oldCorreo, newCorreo, "UPDATE", adminCorreo);

            if (!StringEquals(oldTelefono, newTelefono))
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "Telefono", oldTelefono, newTelefono, "UPDATE", adminCorreo);

            TempData["Ok"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null) return HttpNotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null) return HttpNotFound();

            string correoAnterior = usuario.Correo;

            usuario.FechaSalida = DateTime.Now;
            usuario.NombreCompleto = "Ex-Empleado #" + usuario.IdUsuario;
            usuario.Correo = $"anon{usuario.IdUsuario}@system.local";
            usuario.Telefono = null;
            usuario.Activo = false;

            db.SaveChanges();

            string adminCorreo = (User?.Identity?.Name ?? "").Trim();
            AuditarCambio("MH_Usuario_TB", id, "Usuario", correoAnterior, null, "DELETE", adminCorreo);

            TempData["Ok"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }

        private void AuditarCambio(string tabla, int idRegistro, string campo, string valorAnterior, string valorNuevo, string accion, string realizadoPor)
        {
            var pTabla = new SqlParameter("@Tabla", tabla);
            var pIdRegistro = new SqlParameter("@IdRegistro", idRegistro);
            var pCampo = new SqlParameter("@Campo", campo);
            var pAnterior = new SqlParameter("@ValorAnterior", (object)(valorAnterior ?? ""));
            var pNuevo = new SqlParameter("@ValorNuevo", (object)(valorNuevo ?? ""));
            var pAccion = new SqlParameter("@Accion", accion);
            var pRealizadoPor = new SqlParameter("@RealizadoPor", (object)(realizadoPor ?? ""));

            db.Database.ExecuteSqlCommand(
                "EXEC SP_MH_Auditoria_Insert @Tabla, @IdRegistro, @Campo, @ValorAnterior, @ValorNuevo, @Accion, @RealizadoPor",
                pTabla, pIdRegistro, pCampo, pAnterior, pNuevo, pAccion, pRealizadoPor
            );
        }

        private bool StringEquals(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}