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

        public ActionResult Crear()
        {
            var usuario = ObtenerUsuarioActual();

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            if (usuario.IdTipoUsuario != 2 && usuario.IdTipoUsuario != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Mascotas = new SelectList(_db.MH_Mascotas_TB, "IdMascota", "NombreMascota");
            ViewBag.Citas = new SelectList(_db.MH_Cita_TB
                    .ToList() // 
                    .Select(c => new {
                        c.IdCita,
                        Texto = c.FechaHoraCita.ToString("dd/MM/yyyy HH:mm")
                    }),
    "IdCita","Texto");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(MH_Recomendaciones_TB model)
        {
            var usuario = ObtenerUsuarioActual();

            // 🔐 Validar sesión
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            // 🔐 Validar rol (2 = Admin, 3 = Empleado)
            if (usuario.IdTipoUsuario != 2 && usuario.IdTipoUsuario != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            // ❌ Si el modelo no es válido, regresar a la vista
           
                if (!ModelState.IsValid)
                {
                    ViewBag.Mascotas = new SelectList(_db.MH_Mascotas_TB, "IdMascota", "NombreMascota");
                    ViewBag.Citas = new SelectList(_db.MH_Cita_TB, "IdCita", "IdCita");

                    return View(model);
                }
           
            // ✅ Asignar datos automáticamente
            model.IdUsuario = usuario.IdUsuario;
            model.Fecha = DateTime.Now;

            _db.MH_Recomendaciones_TB.Add(model);
            _db.SaveChanges();

            // 🧾 Auditoría
            try
            {
                AuditoriaHelper.Registrar(
                    "MH_Recomendaciones_TB",
                    "Recomendaciones",
                    "Crear",
                    "",
                    model.IdRecomendacion.ToString(),
                    "",
                    model.Descripcion,
                    usuario.Correo,
                    "Se creó una recomendación manual"
                );
            }
            catch
            {
                // No romper la app si falla auditoría
            }

            return RedirectToAction("HistorialRecomendaciones");
        }
        private MH_Usuario_TB ObtenerUsuarioActual()
        {
            var correo = User.Identity.Name;
            return _db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
        }
    }
}