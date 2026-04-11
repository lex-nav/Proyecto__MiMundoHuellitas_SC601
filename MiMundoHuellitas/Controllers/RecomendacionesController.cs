using MiMundoHuellitas.EF;
using MiMundoHuellitas.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    public class RecomendacionesController : Controller
    {
        private MiMundoHuellitasEntities _db = new MiMundoHuellitasEntities();

        public ActionResult HistorialRecomendaciones()
        {
            var usuario = ObtenerUsuarioActual();

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var recomendaciones = _db.MH_Recomendaciones_TB
                .Where(r => r.IdUsuario == usuario.IdUsuario)
                .OrderByDescending(r => r.Fecha)
                .ToList();

            return View(recomendaciones);
        }

        private MH_Usuario_TB ObtenerUsuarioActual()
        {
            var correo = User.Identity.Name;
            return _db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
        }
    }
}