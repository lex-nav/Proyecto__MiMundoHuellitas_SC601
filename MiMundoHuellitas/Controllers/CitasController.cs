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
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

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

                var mail = new MailMessage(user, destino);
                mail.Subject = asunto;
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

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

            // Crear cita (Estado: 1 = Pendiente, ajusta según tus datos)
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

            // Correo de confirmación
            try
            {
                // Cargar mascota explícitamente
                var mascota = db.MH_Mascotas_TB.FirstOrDefault(m => m.IdMascota == cita.IdMascota);

                string cuerpo = $@"
        <p>Hola {(string.IsNullOrWhiteSpace(usuario.NombreCompleto) ? usuario.Correo : usuario.NombreCompleto)},</p>
        <p>Tu cita ha sido agendada correctamente:</p>
        <ul>
            <li><strong>Fecha:</strong> {cita.FechaHoraCita:dd/MM/yyyy HH:mm}</li>
            <li><strong>Mascota:</strong> {(mascota != null ? mascota.NombreMascota : "N/D")}</li>
            <li><strong>Servicio:</strong> {servicio.NombreServicio}</li>
        </ul>
        <p>¡Gracias por confiar en Mi Mundo de Huellitas!</p>";

                EnviarCorreo(usuario.Correo, "Confirmación de cita", cuerpo);
            }
            catch
            {
                // si falla el correo, no rompemos el flujo
            }


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

            // Si cambió fecha/hora, validar que no esté ocupado por otra cita
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

            // Correo de actualización
            try
            {
                string cuerpo = $@"
                    <p>Hola {usuario.NombreCompleto},</p>
                    <p>Tu cita ha sido actualizada:</p>
                    <ul>
                        <li><strong>Fecha:</strong> {cita.FechaHoraCita:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Mascota:</strong> {cita.MH_Mascotas_TB.NombreMascota}</li>
                        <li><strong>Servicio:</strong> {servicio.NombreServicio}</li>
                    </ul>";

                EnviarCorreo(usuario.Correo, "Actualización de cita", cuerpo);
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

            // Marca como cancelada (ajusta el IdEstado real que uses para "Cancelada")
            cita.IdEstado = 3;
            cita.FechaActualiza = DateTime.Now;
            db.SaveChanges();

            try
            {
                string cuerpo = $@"
                    <p>Hola {usuario.NombreCompleto},</p>
                    <p>Tu cita para el {cita.FechaHoraCita:dd/MM/yyyy HH:mm} ha sido <strong>cancelada</strong>.</p>
                    <p>Si fue un error, puedes agendar una nueva cita desde el sistema.</p>";

                EnviarCorreo(usuario.Correo, "Cita cancelada", cuerpo);
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
