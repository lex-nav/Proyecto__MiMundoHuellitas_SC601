using MiMundoHuellitas.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    [Authorize(Roles = "Empleado,Administrador")]
    public class MarcacionesController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        private MH_Usuario_TB UsuarioActual()
        {
            var correo = (User?.Identity?.Name ?? "").Trim();
            return db.MH_Usuario_TB.FirstOrDefault(u => u.Activo && u.Correo == correo);
        }

        public ActionResult Index()
        {
            var user = UsuarioActual();
            if (user == null) return RedirectToAction("Login", "Account");

            var hoy = DateTime.Today;

            var marcacion = db.MH_Marcacion_TB
                .FirstOrDefault(m => m.IdUsuario == user.IdUsuario && m.Fecha == hoy);

            ViewBag.Marcacion = marcacion;
            ViewBag.Jornada = null; // temporal hasta alinear el modelo con jornadas

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarEntrada()
        {
            var user = UsuarioActual();
            if (user == null) return RedirectToAction("Login", "Account");

            var hoy = DateTime.Today;
            var ahora = DateTime.Now;

            var marcacion = db.MH_Marcacion_TB
                .FirstOrDefault(m => m.IdUsuario == user.IdUsuario && m.Fecha == hoy);

            if (marcacion == null)
            {
                marcacion = new MH_Marcacion_TB
                {
                    IdUsuario = user.IdUsuario,
                    Fecha = hoy,
                    HoraEntrada = ahora,
                    Aprobada = false
                };
                db.MH_Marcacion_TB.Add(marcacion);
            }
            else
            {
                if (marcacion.HoraEntrada != null)
                {
                    TempData["Error"] = "Ya registraste tu entrada hoy.";
                    return RedirectToAction("Index");
                }

                marcacion.HoraEntrada = ahora;
            }

            db.SaveChanges();
            TempData["Ok"] = "Entrada registrada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarSalida()
        {
            var user = UsuarioActual();
            if (user == null) return RedirectToAction("Login", "Account");

            var hoy = DateTime.Today;
            var ahora = DateTime.Now;

            var marcacion = db.MH_Marcacion_TB
                .FirstOrDefault(m => m.IdUsuario == user.IdUsuario && m.Fecha == hoy);

            if (marcacion == null || marcacion.HoraEntrada == null)
            {
                TempData["Error"] = "No puedes marcar salida sin haber marcado entrada.";
                return RedirectToAction("Index");
            }

            if (marcacion.HoraSalida != null)
            {
                TempData["Error"] = "Ya registraste tu salida hoy.";
                return RedirectToAction("Index");
            }

            marcacion.HoraSalida = ahora;

            db.SaveChanges();
            TempData["Ok"] = "Salida registrada correctamente.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}