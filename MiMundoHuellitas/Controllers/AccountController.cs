using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MiMundoHuellitas.Controllers
{
    public class AccountController : Controller
    {
        private BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // ===================== LOGIN =====================
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            string correo = (model.Correo ?? "").Trim().ToLowerInvariant();
            string hash = HashPassword(model.Contrasena ?? "");

            var usuario = db.MH_Usuario_TB
                .FirstOrDefault(u => (u.Correo ?? "").ToLower() == correo && u.Activo == true);

            if (usuario == null ||
                !string.Equals(usuario.ContrasennaHash, hash, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            // Rol fijo por ahora (si luego mapeas desde tabla, aquí lo cambias)
            string rol = "Cliente";

            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                model.Recordarme,
                rol
            );

            var cookie = new HttpCookie(
                FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true
            };

            if (model.Recordarme)
                cookie.Expires = ticket.Expiration;

            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ===================== REGISTER =====================
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string correo = (model.Correo ?? "").Trim().ToLowerInvariant();

            if (db.MH_Usuario_TB.Any(u => (u.Correo ?? "").ToLower() == correo))
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario con ese correo.");
                return View(model);
            }

            var usuario = new MH_Usuario_TB
            {
                IdTipoUsuario = 1,
                NombreCompleto = (model.Nombre ?? "").Trim(),
                Correo = correo,
                Telefono = (model.Telefono ?? "").Trim(),
                ContrasennaHash = HashPassword(model.Contrasena ?? ""),
                Activo = true
            };

            db.MH_Usuario_TB.Add(usuario);
            db.SaveChanges();

            // Auto-login al registrar
            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                false,
                "Cliente"
            );

            Response.Cookies.Add(new HttpCookie(
                FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(ticket))
            { HttpOnly = true });

            return RedirectToAction("Index", "Home");
        }

        // ===================== RECUPERAR CONTRASEÑA =====================
        [AllowAnonymous]
        [HttpGet]
        public ActionResult RecuperarContrasena()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasena(RecuperarContrasenaViewModel model)
        {

            // ✅ Respaldo por si el binder falla (inputs renameados, etc.)
            string correoPosted = (Request.Form["Correo"] ?? model?.Correo ?? "").Trim();
            string nombrePosted = (Request.Form["Nombre"] ?? model?.Nombre ?? "").Trim();

            model = model ?? new RecuperarContrasenaViewModel();
            model.Correo = correoPosted;
            model.Nombre = nombrePosted;

            // ✅ Validación manual clara
            if (string.IsNullOrWhiteSpace(model.Correo) || string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("", "Debe completar todos los campos.");
                return View(model);
            }

            // Validaciones de DataAnnotations (EmailAddress, etc.)
            TryValidateModel(model);
            if (!ModelState.IsValid)
                return View(model);

            string correoNorm = NormalizarTexto(model.Correo);
            string nombreNorm = NormalizarTexto(model.Nombre);

            // ✅ Buscar primero por correo
            var usuario = db.MH_Usuario_TB.FirstOrDefault(u =>
                u.Activo == true &&
                (u.Correo ?? "").ToLower() == correoNorm
            );

            if (usuario == null)
            {
                ModelState.AddModelError("", "No se encontró un usuario con ese correo.");
                return View(model);
            }

            // ✅ Validar nombre tolerante (tildes/espacios/mayúsculas)
            string nombreDbNorm = NormalizarTexto(usuario.NombreCompleto ?? "");
            if (nombreDbNorm != nombreNorm)
            {
                ModelState.AddModelError("", "El nombre no coincide con el registrado para ese correo.");
                return View(model);
            }

            // Generar nueva contraseña temporal
            string nuevaPass = Guid.NewGuid().ToString("N").Substring(0, 8);
            usuario.ContrasennaHash = HashPassword(nuevaPass);
            db.SaveChanges();

            try
            {
                string html = GenerarCorreoRecuperacion(usuario.NombreCompleto, nuevaPass);

                EnviarCorreo(
                    usuario.Correo,
                    "Recuperación de contraseña - Mi Mundo de Huellitas",
                    html
                );

                TempData["Ok"] = "Se envió una nueva contraseña a tu correo.";
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError("", "No se pudo enviar el correo. Verifica la configuración SMTP.");
                return View(model);
            }
        }

        // ===================== LOGOUT =====================
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // ===================== UTILIDADES =====================

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password ?? "");
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", "").ToLowerInvariant();
            }
        }

        private void EnviarCorreo(string destino, string asunto, string mensajeHtml)
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
                    Body = mensajeHtml,
                    IsBodyHtml = true
                };

                smtp.Send(mail);
            }
        }

        // ✅ Normaliza: lower, sin tildes, colapsa espacios
        private string NormalizarTexto(string input)
        {
            input = (input ?? "").Trim().ToLowerInvariant();

            // quitar tildes
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var sinTildes = sb.ToString().Normalize(NormalizationForm.FormC);

            // colapsar espacios múltiples
            sinTildes = Regex.Replace(sinTildes, @"\s+", " ").Trim();

            return sinTildes;
        }

        // ✅ HTML bonito para recuperación
        private string GenerarCorreoRecuperacion(string nombre, string nuevaPass)
        {
            string n = string.IsNullOrWhiteSpace(nombre) ? "cliente" : nombre.Trim();

            return $@"
<!DOCTYPE html>
<html lang='es'>
<body style='margin:0;background:#f4f7f6;font-family:Arial,Helvetica,sans-serif'>
<table width='100%' cellpadding='0' cellspacing='0' style='padding:30px 0'>
<tr><td align='center'>
<table width='600' style='background:#fff;border-radius:14px;overflow:hidden;box-shadow:0 6px 18px rgba(0,0,0,.08)'>

<tr>
<td style='background:linear-gradient(135deg,#0a7b48,#0e768c);padding:25px;text-align:center;color:#fff'>
  <h1 style='margin:0;font-size:22px'>Mi Mundo de Huellitas</h1>
  <p style='margin:6px 0 0;font-size:14px'>Recuperación de contraseña</p>
</td>
</tr>

<tr>
<td style='padding:30px'>
  <p style='margin-top:0'>Hola <strong>{System.Net.WebUtility.HtmlEncode(n)}</strong>,</p>

  <p>Hemos generado una contraseña temporal para que puedas acceder a tu cuenta:</p>

  <div style='background:#f1fdf8;border:1px dashed #0a7b48;padding:18px;margin:20px 0;text-align:center;border-radius:10px'>
    <div style='font-size:22px;font-weight:bold;color:#0a7b48;letter-spacing:1px'>
      {System.Net.WebUtility.HtmlEncode(nuevaPass)}
    </div>
  </div>

  <p>Te recomendamos cambiarla inmediatamente después de iniciar sesión.</p>

  <p style='margin:22px 0 0'>
    🐾 <strong>Equipo Mi Mundo de Huellitas</strong>
  </p>
</td>
</tr>

<tr>
<td style='background:#f8f8f8;padding:15px;text-align:center;font-size:12px;color:#777'>
  © {DateTime.Now.Year} Mi Mundo de Huellitas · Costa Rica
</td>
</tr>

</table>
</td></tr>
</table>
</body>
</html>";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
