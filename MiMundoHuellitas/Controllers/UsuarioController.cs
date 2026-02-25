using System;
using System.Linq;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities _db = new BD_MiMundoHuellitasEntities();

        // GET: /Usuario/Perfil
        public ActionResult Perfil()
        {
            var correo = User.Identity.Name;

            var usuario = _db.MH_Usuario_TB
                             .Include("MH_Direccion_TB")
                             .FirstOrDefault(u => u.Correo == correo);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var modelo = new PerfilViewModel
            {
                Id = usuario.IdUsuario,
                Nombre = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = "Cliente",
                Telefono = usuario.Telefono,
                IdDireccion = usuario.IdDireccion,
                DireccionDetalle = usuario.MH_Direccion_TB?.Detalle,
                CodigoPostal = usuario.MH_Direccion_TB?.CodigoPostal,
                IdDistrito = usuario.MH_Direccion_TB?.IdDistrito
            };

            return View(modelo);
        }

        // GET: /Usuario/EditarPerfil
        [HttpGet]
        public ActionResult EditarPerfil()
        {
            var correo = User.Identity.Name;

            var usuario = _db.MH_Usuario_TB
                             .Include("MH_Direccion_TB")
                             .FirstOrDefault(u => u.Correo == correo);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var modelo = new PerfilViewModel
            {
                Id = usuario.IdUsuario,
                Nombre = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = "Cliente",
                Telefono = usuario.Telefono,
                IdDireccion = usuario.IdDireccion,
                DireccionDetalle = usuario.MH_Direccion_TB?.Detalle,
                CodigoPostal = usuario.MH_Direccion_TB?.CodigoPostal,
                IdDistrito = usuario.MH_Direccion_TB?.IdDistrito
            };

            return View(modelo);
        }

        // POST: /Usuario/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPerfil(PerfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = _db.MH_Usuario_TB
                             .FirstOrDefault(u => u.IdUsuario == model.Id);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            // ---- Actualizar datos generales ----
            usuario.NombreCompleto = model.Nombre;
            usuario.Telefono = model.Telefono;

            // ---- Manejar DIRECCIÓN ----
            MH_Direccion_TB direccion = null;

            // Si NO tiene dirección → crear una nueva
            if (usuario.IdDireccion == null)
            {
                direccion = new MH_Direccion_TB
                {
                    Detalle = model.DireccionDetalle,
                    CodigoPostal = model.CodigoPostal,
                    IdDistrito = model.IdDistrito ?? 1  // default/temporal
                };

                _db.MH_Direccion_TB.Add(direccion);
                _db.SaveChanges();

                // enlazar usuario → nueva dirección
                usuario.IdDireccion = direccion.IdDireccion;
            }
            else
            {
                // Ya tiene → actualizar
                direccion = _db.MH_Direccion_TB
                               .FirstOrDefault(d => d.IdDireccion == usuario.IdDireccion);

                if (direccion != null)
                {
                    direccion.Detalle = model.DireccionDetalle;
                    direccion.CodigoPostal = model.CodigoPostal;
                    direccion.IdDistrito = model.IdDistrito ?? direccion.IdDistrito;
                }
            }

            _db.SaveChanges();

            TempData["PerfilOk"] = "Tus datos han sido actualizados.";
            return RedirectToAction("Perfil");
        }

        // ==========================
        //      CAMBIAR CONTRASEÑA
        // ==========================

        // GET: /Usuario/CambiarContrasena
        [HttpGet]
        public ActionResult CambiarContrasena()
        {
            var correo = User.Identity.Name;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("Login", "Account");

            var modelo = new CambiarContrasenaViewModel();
            return View(modelo);
        }

        // POST: /Usuario/CambiarContrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var correo = User.Identity.Name;

            var usuario = _db.MH_Usuario_TB
                             .FirstOrDefault(u => u.Correo == correo);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            // Validar contraseña actual
            string hashActual = HashPassword(model.ContrasenaActual);
            if (!string.Equals(usuario.ContrasennaHash, hashActual, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "La contraseña actual no es correcta.");
                return View(model);
            }

            // Actualizar a la nueva
            usuario.ContrasennaHash = HashPassword(model.NuevaContrasena);
            _db.SaveChanges();

            TempData["PerfilOk"] = "Tu contraseña se actualizó correctamente.";
            return RedirectToAction("Perfil");
        }

        // Utilidad para hashear
        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
