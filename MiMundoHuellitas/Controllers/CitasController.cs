using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly MiMundoHuellitasEntities db = new MiMundoHuellitasEntities();

        // =========================
        // Helpers
        // =========================

        private MH_Usuario_TB ObtenerUsuarioActual()
        {
            var correo = User.Identity.Name;
            return db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
        }

        private void CargarCombos(AgendarCitaViewModel model, int idUsuario)
        {
            model.Mascotas = db.MH_Mascotas_TB
                .Where(m => m.IdUsuario == idUsuario && m.Activo)
                .Select(m => new SelectListItem
                {
                    Value = m.IdMascota.ToString(),
                    Text = m.NombreMascota
                })
                .ToList();

            model.Servicios = db.MH_Servicios_TB
                .Where(s => s.Activo)
                .Select(s => new SelectListItem
                {
                    Value = s.IdServicio.ToString(),
                    Text = s.NombreServicio
                })
                .ToList();

            model.Horarios = GenerarHorarios();
        }

        // 08:00, 08:30, ..., 16:00 (cada 30 minutos)
        private IEnumerable<SelectListItem> GenerarHorarios()
        {
            var lista = new List<SelectListItem>();
            var inicio = new TimeSpan(8, 0, 0);
            var fin = new TimeSpan(16, 0, 0);

            for (var t = inicio; t <= fin; t = t.Add(new TimeSpan(0, 30, 0)))
            {
                var texto = DateTime.Today.Date.Add(t).ToString("HH:mm");
                lista.Add(new SelectListItem
                {
                    Value = texto,
                    Text = texto
                });
            }

            return lista;
        }

        private bool EsHorarioValido(DateTime fecha, TimeSpan hora, out string mensajeError)
        {
            mensajeError = null;
            var dia = fecha.DayOfWeek;
            var inicio = new TimeSpan(8, 0, 0);

            if (dia == DayOfWeek.Sunday)
            {
                mensajeError = "No se atienden citas los domingos.";
                return false;
            }

            if (dia == DayOfWeek.Saturday)
            {
                var finSabado = new TimeSpan(11, 30, 0);
                if (hora < inicio || hora > finSabado)
                {
                    mensajeError = "Los sábados el horario es de 8:00 a 11:30.";
                    return false;
                }
            }
            else
            {
                var finSemana = new TimeSpan(16, 0, 0);
                if (hora < inicio || hora > finSemana)
                {
                    mensajeError = "El horario de lunes a viernes es de 8:00 a 16:00.";
                    return false;
                }
            }

            return true;
        }

        private void EnviarCorreo(string destino, string asunto, string cuerpoHtml)
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
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };

                smtp.Send(mail);
            }
        }

        private DateTime? CombinarFechaHora(AgendarCitaViewModel model, out string error)
        {
            error = null;

            if (!model.Fecha.HasValue)
            {
                error = "Debe seleccionar una fecha.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(model.Hora))
            {
                error = "Debe seleccionar una hora.";
                return null;
            }

            if (!TimeSpan.TryParse(model.Hora, out var hora))
            {
                error = "La hora seleccionada no es válida.";
                return null;
            }

            var fecha = model.Fecha.Value.Date;

            if (!EsHorarioValido(fecha, hora, out var msg))
            {
                error = msg;
                return null;
            }

            return fecha.Add(hora);
        }



        private string EmailLayout(string titulo, string subtitulo, string contenidoHtml)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<body style='margin:0;background:#f4f7f6;font-family:Arial,Helvetica,sans-serif'>
<table width='100%' cellpadding='0' cellspacing='0' style='padding:30px 0'>
<tr><td align='center'>
<table width='600' style='background:#fff;border-radius:14px;overflow:hidden;
box-shadow:0 6px 18px rgba(0,0,0,.08)'>

<tr>
<td style='background:linear-gradient(135deg,#0a7b48,#0e768c);
padding:25px;text-align:center;color:#fff'>
<h1 style='margin:0;font-size:22px'>{titulo}</h1>
<p style='margin:6px 0 0;font-size:14px'>{subtitulo}</p>
</td>
</tr>

<tr>
<td style='padding:30px; color:#222; font-size:14px; line-height:1.55'>
{contenidoHtml}
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

        private string EmailConfirmacionCita(string nombre, DateTime fechaHora, string mascota, string servicio)
        {
            var saludo = string.IsNullOrWhiteSpace(nombre) ? "Hola 👋" : $"Hola <strong>{nombre}</strong> 👋";

            string contenido = $@"
<p>{saludo}</p>
<p>¡Tu cita ha sido <strong>agendada correctamente</strong>! 🐾</p>

<div style='background:#f1fdf8;border:1px solid rgba(10,123,72,.20);
padding:16px;border-radius:12px;margin:16px 0'>
<p style='margin:0 0 8px'><strong>📅 Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</p>
<p style='margin:0 0 8px'><strong>🐶 Mascota:</strong> {WebUtility.HtmlEncode(mascota ?? "N/D")}</p>
<p style='margin:0'><strong>✨ Servicio:</strong> {WebUtility.HtmlEncode(servicio ?? "N/D")}</p>
</div>

<p style='margin:0'>Si necesitas reprogramar o cancelar, podés hacerlo desde <strong>Mis citas</strong>.</p>
<p style='margin-top:16px'>¡Gracias por confiar en <strong>Mi Mundo de Huellitas</strong>! 💚</p>";

            return EmailLayout("Mi Mundo de Huellitas", "Confirmación de cita", contenido);
        }

        private string EmailActualizacionCita(string nombre, DateTime fechaHora, string mascota, string servicio)
        {
            var saludo = string.IsNullOrWhiteSpace(nombre) ? "Hola 👋" : $"Hola <strong>{nombre}</strong> 👋";

            string contenido = $@"
<p>{saludo}</p>
<p>Tu cita fue <strong>actualizada</strong> correctamente ✅</p>

<div style='background:#eef7ff;border:1px solid rgba(14,118,140,.20);
padding:16px;border-radius:12px;margin:16px 0'>
<p style='margin:0 0 8px'><strong>📅 Nueva fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</p>
<p style='margin:0 0 8px'><strong>🐶 Mascota:</strong> {WebUtility.HtmlEncode(mascota ?? "N/D")}</p>
<p style='margin:0'><strong>✨ Servicio:</strong> {WebUtility.HtmlEncode(servicio ?? "N/D")}</p>
</div>

<p style='margin:0'>Si algo no quedó correcto, podés editarla nuevamente desde <strong>Mis citas</strong>.</p>
<p style='margin-top:16px'>— Equipo <strong>Mi Mundo de Huellitas</strong> 🐾</p>";

            return EmailLayout("Mi Mundo de Huellitas", "Actualización de cita", contenido);
        }

        private string EmailCancelacionCita(string nombre, DateTime fechaHora)
        {
            var saludo = string.IsNullOrWhiteSpace(nombre) ? "Hola 👋" : $"Hola <strong>{nombre}</strong> 👋";

            string contenido = $@"
<p>{saludo}</p>
<p>Tu cita fue <strong>cancelada</strong> correctamente.</p>

<div style='background:#fff3f3;border:1px solid rgba(220,53,69,.20);
padding:16px;border-radius:12px;margin:16px 0'>
<p style='margin:0'><strong>📅 Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</p>
</div>

<p style='margin:0'>Si fue un error, podés agendar una nueva cita cuando gustés.</p>
<p style='margin-top:16px'>Gracias por usar <strong>Mi Mundo de Huellitas</strong> 💚</p>";

            return EmailLayout("Mi Mundo de Huellitas", "Cita cancelada", contenido);
        }

        // =========================
        // AGENDAR CITA
        // =========================

        // GET: /Citas/Agendar
        [HttpGet]
        public ActionResult Agendar()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var model = new AgendarCitaViewModel
            {
                Fecha = DateTime.Today
            };

            CargarCombos(model, usuario.IdUsuario);
            return View("Agendar", model);
        }

        // POST: /Citas/Agendar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agendar(AgendarCitaViewModel model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                CargarCombos(model, usuario.IdUsuario);
                return View("Agendar", model);
            }

            var fechaHora = CombinarFechaHora(model, out var errorHorario);
            if (!fechaHora.HasValue)
            {
                ModelState.AddModelError("Hora", errorHorario);
                CargarCombos(model, usuario.IdUsuario);
                return View("Agendar", model);
            }

            // Validar que el horario no esté ocupado
            bool ocupado = db.MH_Cita_TB.Any(c => c.FechaHoraCita == fechaHora.Value);
            if (ocupado)
            {
                ModelState.AddModelError("", "Ese horario ya está ocupado, por favor elige otro.");
                CargarCombos(model, usuario.IdUsuario);
                return View("Agendar", model);
            }

            // Crear cita (Estado: 1 = Pendiente)
            var cita = new MH_Cita_TB
            {
                IdUsuario = usuario.IdUsuario,
                IdMascota = model.IdMascota.Value,
                IdEstado = 1,
                FechaHoraCita = fechaHora.Value,
                NotasCliente = model.NotasCliente,
                FechaCreacion = DateTime.Now
            };

            db.MH_Cita_TB.Add(cita);
            db.SaveChanges();

            // Detalle de servicio (1 servicio por cita)
            var servicio = db.MH_Servicios_TB.First(s => s.IdServicio == model.IdServicio.Value);

            var det = new MH_Servicios_Cita_TB
            {
                IdCita = cita.IdCita,
                IdServicio = servicio.IdServicio,
                Cantidad = 1,
                PrecioUnitario = servicio.Precio,
                Subtotal = servicio.Precio
            };

            db.MH_Servicios_Cita_TB.Add(det);
            db.SaveChanges();

            // Correo de confirmación (bonito)
            try
            {
                var mascota = db.MH_Mascotas_TB.FirstOrDefault(m => m.IdMascota == cita.IdMascota);
                var nombreMostrar = string.IsNullOrWhiteSpace(usuario.NombreCompleto) ? usuario.Correo : usuario.NombreCompleto;

                string html = EmailConfirmacionCita(
                    nombreMostrar,
                    cita.FechaHoraCita,
                    mascota?.NombreMascota ?? "N/D",
                    servicio.NombreServicio
                );

                EnviarCorreo(usuario.Correo, "Confirmación de cita - Mi Mundo de Huellitas", html);
            }
            catch { }

            TempData["CitaOk"] = "¡Cita agendada correctamente!";
            return RedirectToAction("VerCitas");
        }

        // =========================
        // VER CITAS DEL USUARIO
        // =========================

        public ActionResult VerCitas()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var citas = db.MH_Cita_TB
                .Where(c => c.IdUsuario == usuario.IdUsuario)
                .OrderByDescending(c => c.FechaHoraCita)
                .ToList();

            return View("VerCitas", citas);
        }

        // =========================
        // EDITAR CITA
        // =========================

        [HttpGet]
        public ActionResult EditarCitas(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var cita = db.MH_Cita_TB.FirstOrDefault(c => c.IdCita == id && c.IdUsuario == usuario.IdUsuario);
            if (cita == null)
                return HttpNotFound();

            var det = db.MH_Servicios_Cita_TB.FirstOrDefault(d => d.IdCita == cita.IdCita);

            var model = new AgendarCitaViewModel
            {
                IdCita = cita.IdCita,
                IdMascota = cita.IdMascota,
                IdServicio = det?.IdServicio,
                Fecha = cita.FechaHoraCita.Date,
                Hora = cita.FechaHoraCita.ToString("HH:mm"),
                NotasCliente = cita.NotasCliente
            };

            CargarCombos(model, usuario.IdUsuario);
            return View("EditarCitas", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCitas(AgendarCitaViewModel model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                CargarCombos(model, usuario.IdUsuario);
                return View("EditarCitas", model);
            }

            var fechaHora = CombinarFechaHora(model, out var errorHorario);
            if (!fechaHora.HasValue)
            {
                ModelState.AddModelError("Hora", errorHorario);
                CargarCombos(model, usuario.IdUsuario);
                return View("EditarCitas", model);
            }

            var cita = db.MH_Cita_TB.FirstOrDefault(c => c.IdCita == model.IdCita && c.IdUsuario == usuario.IdUsuario);
            if (cita == null)
                return HttpNotFound();

            bool ocupado = db.MH_Cita_TB.Any(c =>
                c.IdCita != cita.IdCita &&
                c.FechaHoraCita == fechaHora.Value);

            if (ocupado)
            {
                ModelState.AddModelError("", "Ese horario ya está ocupado, por favor elige otro.");
                CargarCombos(model, usuario.IdUsuario);
                return View("EditarCitas", model);
            }

            cita.IdMascota = model.IdMascota.Value;
            cita.FechaHoraCita = fechaHora.Value;
            cita.NotasCliente = model.NotasCliente;
            cita.FechaActualiza = DateTime.Now;

            var det = db.MH_Servicios_Cita_TB.FirstOrDefault(d => d.IdCita == cita.IdCita);
            var servicio = db.MH_Servicios_TB.First(s => s.IdServicio == model.IdServicio.Value);

            if (det == null)
            {
                det = new MH_Servicios_Cita_TB
                {
                    IdCita = cita.IdCita,
                    IdServicio = servicio.IdServicio,
                    Cantidad = 1,
                    PrecioUnitario = servicio.Precio,
                    Subtotal = servicio.Precio
                };
                db.MH_Servicios_Cita_TB.Add(det);
            }
            else
            {
                det.IdServicio = servicio.IdServicio;
                det.PrecioUnitario = servicio.Precio;
                det.Subtotal = servicio.Precio * det.Cantidad;
            }

            db.SaveChanges();

            // Correo de actualización (bonito)
            try
            {
                var nombreMostrar = string.IsNullOrWhiteSpace(usuario.NombreCompleto) ? usuario.Correo : usuario.NombreCompleto;

                string html = EmailActualizacionCita(
                    nombreMostrar,
                    cita.FechaHoraCita,
                    cita.MH_Mascotas_TB?.NombreMascota ?? "N/D",
                    servicio.NombreServicio
                );

                EnviarCorreo(usuario.Correo, "Actualización de cita - Mi Mundo de Huellitas", html);
            }
            catch { }

            TempData["CitaOk"] = "La cita ha sido actualizada.";
            return RedirectToAction("VerCitas");
        }

        // =========================
        // CANCELAR CITA
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelarCita(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var cita = db.MH_Cita_TB.FirstOrDefault(c => c.IdCita == id && c.IdUsuario == usuario.IdUsuario);
            if (cita == null)
                return HttpNotFound();

            // Marca como cancelada (ajusta el IdEstado real)
            cita.IdEstado = 3;
            cita.FechaActualiza = DateTime.Now;
            db.SaveChanges();

            // Correo de cancelación (bonito)
            try
            {
                var nombreMostrar = string.IsNullOrWhiteSpace(usuario.NombreCompleto) ? usuario.Correo : usuario.NombreCompleto;

                string html = EmailCancelacionCita(
                    nombreMostrar,
                    cita.FechaHoraCita
                );

                EnviarCorreo(usuario.Correo, "Cita cancelada - Mi Mundo de Huellitas", html);
            }
            catch { }

            TempData["CitaOk"] = "La cita ha sido cancelada.";
            return RedirectToAction("VerCitas");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
