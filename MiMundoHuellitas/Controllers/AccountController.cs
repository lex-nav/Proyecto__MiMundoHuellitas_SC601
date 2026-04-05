// ✅ Ajustá este using al namespace real donde pusiste MarcacionRepository
using MiMundoHuellitas.DAL;
using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TuProyecto.DAL;

namespace MiMundoHuellitas.Controllers
{
    public class AccountController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // ✅ Repositorio de marcación (SP: usp_MH_MarcarEntrada / usp_MH_MarcarSalida)
        private readonly MarcacionRepository _marcRepo = new MarcacionRepository();

        // ✅ IDs según tu BD
        private const int ID_ADMIN = 2;
        private const int ID_CLIENTE = 1;
        private const int ID_EMPLEADO = 3;

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

            // ✅ Rol según IdTipoUsuario
            string rol =
                (usuario.IdTipoUsuario == ID_ADMIN) ? "Admin" :
                (usuario.IdTipoUsuario == ID_EMPLEADO) ? "Empleado" :
                "Cliente";

            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.Correo,                     // User.Identity.Name
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                model.Recordarme,
                rol                                 // ✅ rol (UserData)
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (model.Recordarme)
            {
                cookie.Expires = ticket.Expiration;
            }

            Response.Cookies.Add(cookie);

            // ✅ Guardar info para Logout / otros usos
            Session["IdUsuario"] = usuario.IdUsuario;
            Session["IdTipoUsuario"] = usuario.IdTipoUsuario;

            // ✅ MARCAR ENTRADA AUTOMÁTICA para EMPLEADO (3) o ADMIN (1)
            if (usuario.IdTipoUsuario == ID_EMPLEADO || usuario.IdTipoUsuario == ID_ADMIN)
            {
                try { _marcRepo.MarcarEntrada(usuario.IdUsuario); } catch { }
            }

            {
                try
                {
                    _marcRepo.MarcarEntrada(usuario.IdUsuario);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al marcar entrada: " + ex.Message);
                }
            }

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

            // ✅ Buscar IdTipoUsuario del tipo "Cliente"
            int idTipoCliente = db.MH_Tipo_Usuario_TB
                .Where(t => t.Activo)
                .Where(t => t.NombreTipoUsuario.Trim().Equals("Cliente", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.IdTipoUsuario)
                .FirstOrDefault();

            if (idTipoCliente <= 0) idTipoCliente = ID_CLIENTE;

            var nuevoUsuario = new MH_Usuario_TB
            {
                IdTipoUsuario = idTipoCliente,
                NombreCompleto = (model.Nombre ?? "").Trim(),
                Correo = correo,
                ContrasennaHash = HashPassword(model.Contrasena),
                Telefono = (model.Telefono ?? "").Trim(),
                FechaRegistro = DateTime.Now,
                Activo = true
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

            // ✅ Guardar Session (por consistencia)
            Session["IdUsuario"] = nuevoUsuario.IdUsuario;
            Session["IdTipoUsuario"] = nuevoUsuario.IdTipoUsuario;

            // ❌ NO marcamos entrada porque Register es Cliente

            return RedirectToAction("Index", "Home");
        }

        // =========================
        //         LOGOUT
        // =========================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            int idUsuario = 0;
            int idTipoUsuario = 0;

            if (Session["IdUsuario"] != null)
                int.TryParse(Session["IdUsuario"].ToString(), out idUsuario);

            if (Session["IdTipoUsuario"] != null)
                int.TryParse(Session["IdTipoUsuario"].ToString(), out idTipoUsuario);

            // ✅ Marcar SALIDA para:
            // - Empleado (3)
            // - Admin (1)
            if ((idTipoUsuario == ID_EMPLEADO || idTipoUsuario == ID_ADMIN) && idUsuario > 0)
            {
                try
                {
                    _marcRepo.MarcarSalida(idUsuario);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al marcar salida: " + ex.Message);
                }
            }

            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }



        // =========================
        //   RECUPERAR CONTRASEÑA
        // =========================
        [AllowAnonymous]
        public ActionResult RecuperarContrasena()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasena(RecuperarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Muestra Required / EmailAddress / etc.
                return View(model);
            }

            string correo = (model.Correo ?? "").Trim();
            string nombre = (model.Nombre ?? "").Trim();

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "Debe completar todos los campos.";
                return View(model);
            }

            // ✅ Búsqueda exacta (tu lógica)
            var usuario = db.MH_Usuario_TB
                .FirstOrDefault(u => u.Activo &&
                                     u.Correo == correo &&
                                     u.NombreCompleto == nombre);

            if (usuario == null)
            {
                TempData["Error"] = "No se encontró un usuario con esos datos.";
                return View(model);
            }

            string nuevaPass = Guid.NewGuid().ToString("N").Substring(0, 8);

            usuario.ContrasennaHash = HashPassword(nuevaPass);
            db.SaveChanges();

            // ✅ HTML bonito como tu screenshot
            string html = GenerarHtmlRecuperacion(usuario.NombreCompleto, nuevaPass);

            EnviarCorreo(
                usuario.Correo,
                "Recuperación de Contraseña - Mi Mundo Huellitas",
                html
            );

            TempData["Ok"] = "Se envió una nueva contraseña a tu correo.";
            return RedirectToAction("Login", "Account");
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

        // ✅ Plantilla HTML inline para email (se ve bien en Outlook/Gmail móvil)
        private static string GenerarHtmlRecuperacion(string nombreCompleto, string passTemporal)
        {
            string nombre = WebUtility.HtmlEncode(nombreCompleto ?? "");
            string pass = WebUtility.HtmlEncode(passTemporal ?? "");

            return $@"
<!doctype html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Recuperación de contraseña</title>
</head>
<body style=""margin:0;padding:0;background:#0b0f14;font-family:Arial,Helvetica,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#0b0f14;padding:24px 0;"">
    <tr>
      <td align=""center"">

        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0""
               style=""width:600px;max-width:92vw;background:#121826;border-radius:16px;overflow:hidden;border:1px solid rgba(255,255,255,0.06);"">

          <tr>
            <td style=""background:linear-gradient(90deg,#00c853,#03a9f4);padding:22px 24px;text-align:center;"">
              <div style=""font-size:22px;font-weight:800;color:#0b0f14;letter-spacing:0.3px;"">Mi Mundo de Huellitas</div>
              <div style=""font-size:14px;font-weight:600;color:rgba(11,15,20,0.8);margin-top:4px;"">Recuperación de contraseña</div>
            </td>
          </tr>

          <tr>
            <td style=""padding:26px 26px 10px;color:#e8eef6;"">
              <div style=""font-size:18px;line-height:1.45;margin:0 0 14px;"">
                Hola <strong style=""color:#ffffff;"">{nombre}</strong>,
              </div>

              <div style=""font-size:15px;line-height:1.6;color:rgba(232,238,246,0.85);margin:0 0 18px;"">
                Hemos generado una contraseña temporal para que puedas acceder a tu cuenta:
              </div>

              <div style=""background:#0f172a;border:2px dashed rgba(0,200,83,0.6);border-radius:14px;padding:18px;text-align:center;margin:0 0 18px;"">
                <div style=""font-size:30px;font-weight:900;letter-spacing:2px;color:#9ef6c3;"">{pass}</div>
              </div>

              <div style=""font-size:14px;line-height:1.6;color:rgba(232,238,246,0.8);margin:0 0 12px;"">
                Te recomendamos cambiarla inmediatamente después de iniciar sesión.
              </div>

              <div style=""margin-top:18px;color:rgba(232,238,246,0.75);font-size:14px;"">
                🐾 <strong>Equipo Mi Mundo de Huellitas</strong>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""padding:18px 24px 22px;color:rgba(232,238,246,0.45);font-size:12px;text-align:center;border-top:1px solid rgba(255,255,255,0.06);"">
              © {DateTime.Now.Year} Mi Mundo de Huellitas · Costa Rica
            </td>
          </tr>

        </table>

        <div style=""height:18px;line-height:18px;"">&nbsp;</div>

        <div style=""max-width:600px;color:rgba(232,238,246,0.35);font-size:11px;line-height:1.5;text-align:center;padding:0 18px;"">
          Si tú no solicitaste esta recuperación, puedes ignorar este correo.
        </div>

      </td>
    </tr>
  </table>
</body>
</html>";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
