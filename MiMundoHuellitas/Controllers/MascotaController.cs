using System.Linq;
using System.Web.Mvc;
using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class MascotaController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities _db = new BD_MiMundoHuellitasEntities();

        // =====================
        // Helpers
        // =====================

        private MH_Usuario_TB ObtenerUsuarioActual()
        {
            var correo = User.Identity.Name;
            return _db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
        }

        private void CargarCombosMascota(MascotaViewModel modelo)
        {
            modelo.Especies = _db.MH_Especie_TB
                .Select(e => new SelectListItem
                {
                    Value = e.IdEspecie.ToString(),
                    Text = e.NombreEspecie          // <-- aquí va NombreEspecie
                })
                .ToList();

            modelo.Razas = _db.MH_Raza_TB
                .Select(r => new SelectListItem
                {
                    Value = r.IdRaza.ToString(),
                    Text = r.NombreRaza             // <-- aquí va NombreRaza
                })
                .ToList();
        }

        // =====================
        // LISTAR MIS MASCOTAS
        // =====================
        public ActionResult VerMascotas()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var mascotas = _db.MH_Mascotas_TB
                .Where(m => m.IdUsuario == usuario.IdUsuario && m.Activo)
                .ToList();

            return View(mascotas); // Vista fuertemente tipada a IEnumerable<MH_Mascotas_TB>
        }

        // =====================
        // AGREGAR
        // =====================

        [HttpGet]
        public ActionResult AgregarMascota()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var modelo = new MascotaViewModel
            {
                Activo = true
            };

            CargarCombosMascota(modelo);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarMascota(MascotaViewModel model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                CargarCombosMascota(model);
                return View(model);
            }

            var mascota = new MH_Mascotas_TB
            {
                IdUsuario = usuario.IdUsuario,
                NombreMascota = model.NombreMascota,
                IdEspecie = model.IdEspecie.Value,   // <- aquí el cambio
                IdRaza = model.IdRaza,
                FechaNacimiento = model.FechaNacimiento,
                Observaciones = model.Observaciones,
                Activo = true
            };


            _db.MH_Mascotas_TB.Add(mascota);
            _db.SaveChanges();

            return RedirectToAction("VerMascotas");
        }

        // =====================
        // EDITAR
        // =====================

        [HttpGet]
        public ActionResult EditarMascota(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var mascota = _db.MH_Mascotas_TB
                .FirstOrDefault(m => m.IdMascota == id && m.IdUsuario == usuario.IdUsuario);

            if (mascota == null)
                return HttpNotFound();

            var modelo = new MascotaViewModel
            {
                IdMascota = mascota.IdMascota,
                NombreMascota = mascota.NombreMascota,
                IdEspecie = mascota.IdEspecie,
                IdRaza = mascota.IdRaza,
                FechaNacimiento = mascota.FechaNacimiento,
                Observaciones = mascota.Observaciones,
                Activo = mascota.Activo
            };

            CargarCombosMascota(modelo);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMascota(MascotaViewModel model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                CargarCombosMascota(model);
                return View(model);
            }

            var mascota = _db.MH_Mascotas_TB
                .FirstOrDefault(m => m.IdMascota == model.IdMascota &&
                                     m.IdUsuario == usuario.IdUsuario);

            if (mascota == null)
                return HttpNotFound();

            mascota.NombreMascota = model.NombreMascota;
            mascota.IdEspecie = model.IdEspecie.Value;  // <- aquí el cambio
            mascota.IdRaza = model.IdRaza;
            mascota.FechaNacimiento = model.FechaNacimiento;
            mascota.Observaciones = model.Observaciones;
            mascota.Activo = model.Activo;


            _db.SaveChanges();

            return RedirectToAction("VerMascotas");
        }

        // =====================
        // ELIMINAR (LÓGICO)
        // =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarMascota(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var mascota = _db.MH_Mascotas_TB
                .FirstOrDefault(m => m.IdMascota == id && m.IdUsuario == usuario.IdUsuario);

            if (mascota != null)
            {
                mascota.Activo = false;
                _db.SaveChanges();
            }

            return RedirectToAction("VerMascotas");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
