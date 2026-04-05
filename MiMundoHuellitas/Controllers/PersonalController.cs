using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class PersonalController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        private void SetUsuarioAuditoria()
        {
            var usuarioAuditoria = User.Identity.Name;

            if (string.IsNullOrWhiteSpace(usuarioAuditoria))
                usuarioAuditoria = "UsuarioNoIdentificado";

            db.Database.ExecuteSqlCommand(
                "EXEC sp_set_session_context @key=N'UsuarioAuditoria', @value=@p0;",
                usuarioAuditoria
            );
        }

        // GET: /Personal/LaboralIndex
        public ActionResult LaboralIndex()
        {
            var lista = (
                from el in db.MH_EmpleadoLaboral_TB
                join u in db.MH_Usuario_TB on el.IdUsuario equals u.IdUsuario
                join ej in db.MH_EmpleadoJornada_TB on el.IdUsuario equals ej.IdUsuario into jornadaGroup
                from ej in jornadaGroup.DefaultIfEmpty()
                join j in db.MH_Jornada_TB on ej.IdJornada equals j.IdJornada into jGroup
                from j in jGroup.DefaultIfEmpty()
                orderby u.NombreCompleto
                select new EmpleadoLaboralVM
                {
                    IdEmpleadoLaboral = el.IdEmpleadoLaboral,
                    IdEmpleadoJornada = ej != null ? ej.IdEmpleadoJornada : 0,
                    IdUsuario = el.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    SalarioHora = el.SalarioHora,
                    SalarioMensual = el.SalarioMensual,
                    Activo = el.Activo,
                    IdJornada = ej != null ? ej.IdJornada : 0,
                    NombreJornada = (ej != null && j != null) ? j.Nombre : "Sin jornada"
                }
            ).ToList();

            return View(lista);
        }

        // GET: /Personal/LaboralEdit/5
        public ActionResult LaboralEdit(int id)
        {
            var model = (
                from el in db.MH_EmpleadoLaboral_TB
                join u in db.MH_Usuario_TB on el.IdUsuario equals u.IdUsuario
                join ej in db.MH_EmpleadoJornada_TB on el.IdUsuario equals ej.IdUsuario into jornadaGroup
                from ej in jornadaGroup.DefaultIfEmpty()
                join j in db.MH_Jornada_TB on ej.IdJornada equals j.IdJornada into jGroup
                from j in jGroup.DefaultIfEmpty()
                where el.IdEmpleadoLaboral == id
                select new EmpleadoLaboralVM
                {
                    IdEmpleadoLaboral = el.IdEmpleadoLaboral,
                    IdEmpleadoJornada = ej != null ? ej.IdEmpleadoJornada : 0,
                    IdUsuario = el.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    SalarioHora = el.SalarioHora,
                    SalarioMensual = el.SalarioMensual,
                    Activo = el.Activo,
                    IdJornada = ej != null ? ej.IdJornada : 0,
                    NombreJornada = (ej != null && j != null) ? j.Nombre : "Sin jornada"
                }
            ).FirstOrDefault();

            if (model == null)
                return HttpNotFound();

            ViewBag.Jornadas = new SelectList(
                db.MH_Jornada_TB.OrderBy(x => x.Nombre).ToList(),
                "IdJornada",
                "Nombre",
                model.IdJornada
            );

            return View(model);
        }

        // POST: /Personal/LaboralEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LaboralEdit(EmpleadoLaboralVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Jornadas = new SelectList(
                    db.MH_Jornada_TB.OrderBy(x => x.Nombre).ToList(),
                    "IdJornada",
                    "Nombre",
                    model.IdJornada
                );
                return View(model);
            }

            var empleado = db.MH_EmpleadoLaboral_TB.Find(model.IdEmpleadoLaboral);
            if (empleado == null)
                return HttpNotFound();

            empleado.SalarioHora = model.SalarioHora ?? 0;
            empleado.SalarioMensual = model.SalarioMensual ?? 0;
            empleado.Activo = model.Activo;

            var jornada = db.MH_EmpleadoJornada_TB.FirstOrDefault(x => x.IdUsuario == model.IdUsuario);

            if (jornada != null)
            {
                jornada.IdJornada = model.IdJornada;
            }

            SetUsuarioAuditoria();
            db.SaveChanges();

            TempData["Ok"] = "Datos del personal actualizados correctamente.";
            return RedirectToAction("LaboralIndex");
        }
    }
}