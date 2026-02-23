using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.EF;

namespace MiMundoHuellitas.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsuariosController : Controller
    {
        private readonly MiMundoHuellitas.EF.BD_MiMundoHuellitasEntities db =
            new MiMundoHuellitas.EF.BD_MiMundoHuellitasEntities();

        // GET: /AdminUsuarios
        public ActionResult Index()
        {
            var usuarios = db.MH_Usuario_TB
                .Include(u => u.MH_Tipo_Usuario_TB)
                .Include(u => u.MH_Jornada_TB)
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

            CargarJornadasDropdown(usuario.IdJornada);

            return View(usuario);
        }

        // POST: /AdminUsuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MH_Usuario_TB model)
        {
            if (!ModelState.IsValid)
            {
                // Si el ModelState falla, hay que recargar dropdown para que no se rompa la vista
                CargarJornadasDropdown(model.IdJornada);
                return View(model);
            }

            var usuario = db.MH_Usuario_TB.FirstOrDefault(u => u.IdUsuario == model.IdUsuario);
            if (usuario == null) return HttpNotFound();

            // ====== Snapshot valores anteriores (para comparar/auditar) ======
            string oldNombre = (usuario.NombreCompleto ?? "").Trim();
            string oldCorreo = (usuario.Correo ?? "").Trim();
            string oldTelefono = (usuario.Telefono ?? "").Trim();
            int? oldIdJornada = usuario.IdJornada;

            // ====== Nuevos valores (normalizados) ======
            string newNombre = (model.NombreCompleto ?? "").Trim();
            string newCorreo = (model.Correo ?? "").Trim();
            string newTelefono = (model.Telefono ?? "").Trim();
            int? newIdJornada = model.IdJornada;

            // Quién hizo el cambio (admin logueado)
            string adminCorreo = (User?.Identity?.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(adminCorreo))
                adminCorreo = "admin"; // fallback por si algo raro pasa con auth

            // ====== Aplicar cambios al usuario ======
            usuario.NombreCompleto = newNombre;
            usuario.Correo = newCorreo;
            usuario.Telefono = newTelefono;

            // Jornada: solo tiene sentido asignarla a empleados,
            // pero si quieres permitirla para todos, deja esto tal cual.
            usuario.IdJornada = newIdJornada;

            // Guardar cambios
            db.SaveChanges();

            // ====== Auditoría (solo lo que cambió) ======
            // Tabla: MH_Usuario_TB | IdRegistro: usuario.IdUsuario
            if (!StringEquals(oldNombre, newNombre))
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "NombreCompleto", oldNombre, newNombre, "UPDATE", adminCorreo);

            if (!StringEquals(oldCorreo, newCorreo))
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "Correo", oldCorreo, newCorreo, "UPDATE", adminCorreo);

            if (!StringEquals(oldTelefono, newTelefono))
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "Telefono", oldTelefono, newTelefono, "UPDATE", adminCorreo);

            if (oldIdJornada != newIdJornada)
                AuditarCambio("MH_Usuario_TB", usuario.IdUsuario, "IdJornada",
                    oldIdJornada?.ToString(), newIdJornada?.ToString(), "UPDATE", adminCorreo);

            TempData["Ok"] = "Usuario actualizado correctamente (con auditoría).";
            return RedirectToAction("Index");
        }

        // GET: /AdminUsuarios/Delete/5
        public ActionResult Delete(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null) return HttpNotFound();
            return View(usuario);
        }

        // POST: /AdminUsuarios/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int id)
        {
            var usuario = db.MH_Usuario_TB.Find(id);
            if (usuario == null) return HttpNotFound();

            db.MH_Usuario_TB.Remove(usuario);
            db.SaveChanges();

            // Auditoría de delete (opcional)
            string adminCorreo = (User?.Identity?.Name ?? "").Trim();
            AuditarCambio("MH_Usuario_TB", id, "Usuario", usuario.Correo, null, "DELETE", adminCorreo);

            TempData["Ok"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }

        // =========================================================
        // HELPERS (AQUÍ VAN) ✅
        // =========================================================

        private void CargarJornadasDropdown(int? idJornadaSeleccionada)
        {
            ViewBag.Jornadas = db.MH_Jornada_TB
                .Where(j => j.Activo)
                .OrderBy(j => j.Nombre)
                .Select(j => new SelectListItem
                {
                    Value = j.IdJornada.ToString(),
                    Text = j.Nombre + " (" + j.HoraEntrada + " - " + j.HoraSalida + ")",
                    Selected = (idJornadaSeleccionada.HasValue && idJornadaSeleccionada.Value == j.IdJornada)
                })
                .ToList();
        }

        private void AuditarCambio(string tabla, int idRegistro, string campo, string valorAnterior, string valorNuevo, string accion, string realizadoPor)
        {
            // ⚠️ Ajusta nombres/orden de parámetros EXACTOS según tu SP si difieren
            // Ejemplo esperado:
            // SP_MH_Auditoria_Insert
            //  @Tabla, @IdRegistro, @Campo, @ValorAnterior, @ValorNuevo, @Accion, @RealizadoPor

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
