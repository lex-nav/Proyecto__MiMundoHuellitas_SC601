using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Configuration;
using System.Data.Entity;               // ✅ Include()
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MiMundoHuellitas.Controllers
{
    public class AccountController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // =========================
        //          LOGIN
        // =========================
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            string hash = HashPassword(model.Contrasena);

            // ✅ Traemos el usuario (y tipo por si lo ocupás en otros lados)
            var usuario = db.MH_Usuario_TB
                .Include(u => u.MH_Tipo_Usuario_TB)
                .FirstOrDefault(u =>
                    u.Correo == model.Correo &&
                    u.ContrasennaHash == hash &&
                    u.Activo);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            // ✅ Rol por ID (según tu BD: 1=Admin, 2=Cliente)
            const int ID_ADMIN = 1;
            string rol = (usuario.IdTipoUsuario == ID_ADMIN) ? "Admin" : "Cliente";

            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.Correo,                     // User.Identity.Name
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                model.Recordarme,
                rol                                 // ✅ aquí va el rol (UserData)
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            // ✅ si marca "Recordarme", persistimos la cookie
            if (model.Recordarme)
            {
                cookie.Expires = ticket.Expiration;
            }

            Response.Cookies.Add(cookie);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // =========================
        //        REGISTER
        // =========================
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // ✅ REGISTRO SIEMPRE COMO CLIENTE (nunca Admin)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string correo = (model.Correo ?? "").Trim();

            bool correoExiste = db.MH_Usuario_TB.Any(u => u.Correo == correo);
            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario con ese correo.");
                return View(model);
            }

            // ✅ Buscar IdTipoUsuario del tipo "Cliente" (para no amarrarnos al ID)
            int idTipoCliente = db.MH_Tipo_Usuario_TB
                .Where(t => t.Activo)
                .Where(t => t.NombreTipoUsuario.Trim().Equals("Cliente", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.IdTipoUsuario)
                .FirstOrDefault();

            // Fallback si por alguna razón no existe
            if (idTipoCliente <= 0) idTipoCliente = 2;

            var nuevoUsuario = new MH_Usuario_TB
            {
                IdTipoUsuario = idTipoCliente,
                NombreCompleto = (model.Nombre ?? "").Trim(), // Ajusta si tu VM usa otro nombre
                Correo = correo,
                ContrasennaHash = HashPassword(model.Contrasena),
                Telefono = (model.Telefono ?? "").Trim(),
                FechaRegistro = DateTime.Now,
                Activo = true
                // IdDireccion queda null
            };

            db.MH_Usuario_TB.Add(nuevoUsuario);
            db.SaveChanges();

            // ✅ Opcional: Autologin como Cliente
            var ticket = new FormsAuthenticationTicket(
                1,
                nuevoUsuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                false,
                "Cliente"
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Home");
        }

        // =========================
        //         LOGOUT
        // =========================
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // =========================
        //   RECUPERAR CONTRASEÑA
        // =========================
        [AllowAnonymous]
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasena(string Email, string Nombre)
        {
            Email = (Email ?? "").Trim();
            Nombre = (Nombre ?? "").Trim();

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Nombre))
            {
                TempData["Error"] = "Debe completar todos los campos.";
                return View();
            }

            var usuario = db.MH_Usuario_TB
                .FirstOrDefault(u => u.Activo &&
                                     u.Correo == Email &&
                                     u.NombreCompleto == Nombre);

            if (usuario == null)
            {
                TempData["Error"] = "No se encontró un usuario con esos datos.";
                return View();
            }

            string nuevaPass = Guid.NewGuid().ToString("N").Substring(0, 8);

            usuario.ContrasennaHash = HashPassword(nuevaPass);
            db.SaveChanges();

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

        // =========================
        //        HELPERS
        // =========================
        private static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password ?? "");
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private static void EnviarCorreo(string destino, string asunto, string mensaje)
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

                var mail = new MailMessage(user, destino)
                {
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                smtp.Send(mail);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

