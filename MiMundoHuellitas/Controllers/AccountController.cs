using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;

namespace MiMundoHuellitas.Controllers
{
    public class AccountController : Controller
    {
        private BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            string hash = HashPassword(model.Contrasena);

            // Usamos MH_Usuario_TB
            var usuario = db.MH_Usuario_TB
                .FirstOrDefault(u => u.Correo == model.Correo && u.ContrasennaHash == hash);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            // Por ahora rol fijo; luego podemos mapear desde MH_Tipo_Usuario_TB
            string rol = "Cliente";

            var authTicket = new FormsAuthenticationTicket(
                1,
                usuario.Correo,          // esto será User.Identity.Name
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                model.Recordarme,
                rol
            );

            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Verificar si el correo ya existe en MH_Usuario_TB
            bool correoExiste = db.MH_Usuario_TB.Any(u => u.Correo == model.Correo);
            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario con ese correo.");
                return View(model);
            }

            // 2) Crear el hash de la contraseña
            string hash = HashPassword(model.Contrasena);

            // 3) Crear la entidad MH_Usuario_TB (usa los nombres REALES de tus columnas)
            var nuevoUsuario = new MH_Usuario_TB
            {
                // 1 = Cliente (según lo que definimos en MH_Tipo_Usuario_TB)
                IdTipoUsuario = 1,
                NombreCompleto = model.Nombre,      // o model.NombreCompleto, según tu ViewModel
                Correo = model.Correo,
                ContrasennaHash = hash,
                Telefono = model.Telefono,          // ya soporta teléfono
                Activo = true
                // IdDireccion lo puedes dejar null de momento
            };

            db.MH_Usuario_TB.Add(nuevoUsuario);
            db.SaveChanges();

            // 4) Autenticar de una vez al usuario recién creado
            var authTicket = new FormsAuthenticationTicket(
                1,
                nuevoUsuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                false,                      // recordar o no; puedes usar model.Recordarme si lo tienes
                "Cliente"                   // o el rol que quieras guardar en el ticket
            );

            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // ==== Utilidades ====
        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // GET: Recuperar Contraseña
        [AllowAnonymous]
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        // POST: Recuperar Contraseña
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasena(string Email, string Nombre)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Nombre))
            {
                TempData["Error"] = "Debe completar todos los campos.";
                return View();
            }

            // Ahora usamos la misma BD_MiMundoHuellitasEntities y MH_Usuario_TB
            var usuario = db.MH_Usuario_TB
                .FirstOrDefault(u => u.Correo == Email && u.NombreCompleto == Nombre);

            if (usuario == null)
            {
                TempData["Error"] = "No se encontró un usuario con esos datos.";
                return View();
            }

            // Generar contraseña temporal
            string nuevaPass = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Guardar la nueva contraseña HASHEADA
            usuario.ContrasennaHash = HashPassword(nuevaPass);
            db.SaveChanges();

            // Enviar correo al usuario
            EnviarCorreo(
                usuario.Correo,
                "Recuperación de Contraseña - Mi Mundo Huellitas",
                $"Hola {usuario.NombreCompleto},<br><br>" +
                $"Tu nueva contraseña temporal es:<br><h2>{nuevaPass}</h2><br>" +
                $"Te recomendamos cambiarla después de iniciar sesión."
            );

            TempData["Ok"] = "Se envió una nueva contraseña a tu correo.";
            return RedirectToAction("Login");
        }

        private void EnviarCorreo(string destino, string asunto, string mensaje)
        {
            string user = ConfigurationManager.AppSettings["smtp.user"];
            string pass = ConfigurationManager.AppSettings["smtp.pass"];
            string host = ConfigurationManager.AppSettings["smtp.host"];
            int port = int.Parse(ConfigurationManager.AppSettings["smtp.port"]);
            bool ssl = bool.Parse(ConfigurationManager.AppSettings["smtp.ssl"]);

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials = new NetworkCredential(user, pass);
                smtp.EnableSsl = ssl;

                var mail = new MailMessage(user, destino);
                mail.Subject = asunto;
                mail.Body = mensaje;
                mail.IsBodyHtml = true;

                smtp.Send(mail);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
