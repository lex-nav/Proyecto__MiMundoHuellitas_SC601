using MiMundoHuellitas.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    public class PromocionesController : Controller
    {
        private MiMundoHuellitasEntities db = new MiMundoHuellitasEntities();

        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            // 1. Obtener citas del usuario
            var citas = db.MH_Cita_TB
                .Where(c => c.IdUsuario == usuario.IdUsuario)
                .ToList();

            // 2. Obtener servicios usados
            var serviciosIds = citas
                .SelectMany(c => c.MH_Servicios_Cita_TB)
                .Select(s => s.IdServicio)
                .Distinct()
                .ToList();

            // 3. Generar promociones basadas en esos servicios
            /*var promociones = db.MH_Servicios_TB
                .Where(s => serviciosIds.Contains(s.IdServicio))
                .Select(s => "Descuento especial en " + s.NombreServicio)
                .ToList();

           return View(promociones);*/

            // 3. Generar promociones basadas en esos servicios
            var random = new Random();

            var promociones = db.MH_Servicios_TB
             .Where(s => serviciosIds.Contains(s.IdServicio))
             .SelectMany(s => new List<string>
             {
                "20% de descuento en " + s.NombreServicio,
                "2x1 en " + s.NombreServicio,
                "Descuento VIP en " + s.NombreServicio
             })
             .OrderBy(x => Guid.NewGuid())
             .Take(5)
             .ToList();

            return View(promociones);
        }

        private MH_Usuario_TB ObtenerUsuarioActual()
        {
            var correo = User.Identity.Name;
            return db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
        }
    }
}