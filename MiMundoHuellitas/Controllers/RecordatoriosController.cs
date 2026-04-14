using MiMundoHuellitas.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    public class RecordatoriosController : Controller
    {
        private MiMundoHuellitasEntities db = new MiMundoHuellitasEntities();

        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var hoy = DateTime.Now;

            // Citas futuras
            var citas = db.MH_Cita_TB
                .Where(c => c.IdUsuario == usuario.IdUsuario && c.FechaHoraCita >= hoy)
                .OrderBy(c => c.FechaHoraCita)
                .ToList();

            return View(citas);
        }

        private MH_Usuario_TB ObtenerUsuarioActual()
        {
            var correo = User.Identity.Name;
            return db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
        }
    }
}