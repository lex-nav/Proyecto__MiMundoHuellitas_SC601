using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Configuration;
using MiMundoHuellitas.Models.ViewModels;
using System.Web;
using MiMundoHuellitas.Helpers;

namespace MiMundoHuellitas.Controllers
{
    public class CarritoController : Controller
    {
        private MiMundoHuellitasEntities db = new  MiMundoHuellitasEntities();

        private const string CART_KEY = "CARRITO";

        public ActionResult Catalogo(int page = 1, int pageSize = 10)
        {
            ViewBag.ActiveMenu = "Productos";

            var query = db.MH_Productos_TB
                          .Where(p => p.Activo && p.StockActual > 0)
                          .OrderBy(p => p.IdProducto);

            int totalProductos = query.Count();

            var productos = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProductos / pageSize);

            return View(productos); // Vista: Views/Carrito/Catalogo.cshtml
        }

        // ================== UTILIDADES DE CARRITO ==================
        private List<CarritoItemViewModel> ObtenerCarrito()
        {
            var carrito = Session[CART_KEY] as List<CarritoItemViewModel>;
            if (carrito == null)
            {
                carrito = new List<CarritoItemViewModel>();
                Session[CART_KEY] = carrito;
            }
            return carrito;
        }

        private void GuardarCarrito(List<CarritoItemViewModel> carrito)
        {
            Session[CART_KEY] = carrito;
        }

        // ================== AGREGAR AL CARRITO ==================
        [HttpPost]
        public ActionResult Agregar(int idProducto, int cantidad = 1)
        {
            var producto = db.MH_Productos_TB.Find(idProducto);
            if (producto == null || !producto.Activo)
                return HttpNotFound();

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.IdProducto == idProducto);

            if (item == null)
            {
                carrito.Add(new CarritoItemViewModel
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.NombreProducto,
                    ImagenUrl = producto.ImagenUrl,
                    PrecioUnitario = producto.PrecioUnitario,
                    Cantidad = cantidad
                });
            }
            else
            {
                item.Cantidad += cantidad;
            }

            GuardarCarrito(carrito);
            return RedirectToAction("Cart");
        }

        // ================== VER CARRITO ==================
        // GET: /Carrito/Cart
        public ActionResult Cart()
        {
            var carrito = ObtenerCarrito();
            return View(carrito); // Vista: Views/Carrito/Cart.cshtml
        }

        // ================== EDITAR CARRITO ==================
        [HttpPost]
        public ActionResult ActualizarCantidad(int idProducto, int cantidad)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.IdProducto == idProducto);

            if (item != null)
            {
                if (cantidad <= 0)
                    carrito.Remove(item);
                else
                    item.Cantidad = cantidad;
            }

            GuardarCarrito(carrito);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult Eliminar(int idProducto)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.IdProducto == idProducto);

            if (item != null)
                carrito.Remove(item);

            GuardarCarrito(carrito);
            return RedirectToAction("Cart");
        }

        // ================== CHECKOUT ==================
        // GET: /Carrito/Checkout
        public ActionResult Checkout()
        {
            var carrito = ObtenerCarrito();
            if (!carrito.Any())
                return RedirectToAction("Catalogo");

            var model = new CheckoutViewModel
            {
                Items = carrito,
                MontoTotal = carrito.Sum(i => i.Subtotal),
                CorreoCliente = Session["CorreoUsuario"] as string
            };

            return View(model); // Vista: Views/Carrito/Checkout.cshtml
        }

        // POST: /Carrito/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            var carrito = ObtenerCarrito();
            if (!carrito.Any())
            {
                ModelState.AddModelError("", "El carrito está vacío.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.Items = carrito;
                model.MontoTotal = carrito.Sum(i => i.Subtotal);
                return View(model);
            }

            int idUsuario = 0;

            if (Session["IdUsuario"] != null)
            {
                int.TryParse(Session["IdUsuario"].ToString(), out idUsuario);
            }

            if (idUsuario == 0)
            {
                string correo = Session["CorreoUsuario"] as string;

                if (string.IsNullOrEmpty(correo))
                {
                    correo = model.CorreoCliente;
                }

                if (!string.IsNullOrEmpty(correo))
                {
                    var usuario = db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo);
                    if (usuario != null)
                    {
                        idUsuario = usuario.IdUsuario;
                    }
                }
            }

            if (idUsuario == 0)
            {
                var usuarioDefault = db.MH_Usuario_TB.FirstOrDefault();
                if (usuarioDefault != null)
                {
                    idUsuario = usuarioDefault.IdUsuario;
                }
            }

            if (idUsuario == 0)
            {
                ModelState.AddModelError("", "No se pudo asociar la compra a un usuario válido.");
                model.Items = carrito;
                model.MontoTotal = carrito.Sum(i => i.Subtotal);
                return View(model);
            }

            var factura = new MH_Factura_TB
            {
                IdUsuario = idUsuario,
                FechaFactura = DateTime.Now,
                IdEstado = 1,
                MetodoPago = model.MetodoPago,
                NumeroReferencia = model.NumeroReferencia,
                MontoTotal = carrito.Sum(i => i.Subtotal),
                Observaciones = model.Observaciones
            };

            db.MH_Factura_TB.Add(factura);
            db.SaveChanges();

            foreach (var item in carrito)
            {
                var detalle = new MH_DetalleFactura_TB
                {
                    IdFactura = factura.IdFactura,
                    IdProducto = item.IdProducto,
                    Descripcion = item.NombreProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal
                };
                db.MH_DetalleFactura_TB.Add(detalle);

                var prod = db.MH_Productos_TB.Find(item.IdProducto);
                if (prod != null)
                    prod.StockActual -= item.Cantidad;
            }

            db.SaveChanges();

            // Enviar comprobante
            try
            {
                // Si el modelo no trae correo (por ejemplo porque se precarga desde sesión),
                // intentamos usar el de sesión:
                if (string.IsNullOrWhiteSpace(model.CorreoCliente))
                    model.CorreoCliente = Session["CorreoUsuario"] as string;

                EnviarComprobante(model.CorreoCliente, factura, carrito);
            }
            catch (Exception ex)
            {
                TempData["ErrorCorreo"] = "No se pudo enviar el comprobante: " + ex.Message;
            }

            // Limpiar carrito
            Session.Remove(CART_KEY);

            return RedirectToAction("Confirmacion", new { id = factura.IdFactura });
        }

        // GET: /Carrito/Confirmacion/5
        public ActionResult Confirmacion(int id)
        {
            var factura = db.MH_Factura_TB
                .FirstOrDefault(f => f.IdFactura == id);

            if (factura == null)
                return HttpNotFound();

            return View(factura);
        }

        // ================== CORREO ==================
        private void EnviarComprobante(string correoDestino, MH_Factura_TB factura, List<CarritoItemViewModel> items)
        {
            if (string.IsNullOrWhiteSpace(correoDestino))
                return;

            // Paleta (alineada con tu top bar)
            string color1 = "#03a9f4"; // celeste
            string color2 = "#00e676"; // verde
            string dark = "#0b0f14";
            string muted = "#6b7280";
            string bg = "#f6f7fb";
            string card = "#ffffff";
            string border = "#e5e7eb";

            string fecha = factura.FechaFactura.ToString("dd/MM/yyyy HH:mm");

            // Para que Outlook/Gmail no rompan caracteres
            string metodoPago = HtmlSafe(factura.MetodoPago);
            string referencia = HtmlSafe(factura.NumeroReferencia);
            string obs = HtmlSafe(factura.Observaciones);

            // Tabla filas
            var rows = new StringBuilder();
            foreach (var i in items)
            {
                rows.AppendLine($@"
                    <tr>
                        <td style='padding:12px;border-top:1px solid {border};'>
                            <div style='font-weight:700;color:{dark};'>{HtmlSafe(i.NombreProducto)}</div>
                        </td>
                        <td style='padding:12px;border-top:1px solid {border};text-align:center;color:{dark};'>
                            {i.Cantidad}
                        </td>
                        <td style='padding:12px;border-top:1px solid {border};text-align:right;color:{dark};'>
                            {i.PrecioUnitario.ToString("C")}
                        </td>
                        <td style='padding:12px;border-top:1px solid {border};text-align:right;font-weight:700;color:{dark};'>
                            {i.Subtotal.ToString("C")}
                        </td>
                    </tr>");
            }

            string preheader = $"Tu comprobante #{factura.IdFactura} - Mi Mundo de Huellitas";

            string asunto = $"Comprobante de compra #{factura.IdFactura} - Mi Mundo de Huellitas";

            // HTML email (muy compatible: todo a base de tablas + inline styles)
            string cuerpo = $@"
<!doctype html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1'>
  <title>{asunto}</title>
</head>
<body style='margin:0;padding:0;background:{bg};font-family:Arial, Helvetica, sans-serif;'>
  <!-- Preheader (texto preview, escondido) -->
  <div style='display:none;font-size:1px;color:{bg};line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;'>
    {preheader}
  </div>

  <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background:{bg};padding:28px 12px;'>
    <tr>
      <td align='center'>

        <!-- Contenedor -->
        <table role='presentation' width='640' cellpadding='0' cellspacing='0' style='width:100%;max-width:640px;border-collapse:collapse;'>
          
          <!-- Header gradiente -->
          <tr>
            <td style='border-radius:18px 18px 0 0;overflow:hidden;'>
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;'>
                <tr>
                  <td style='padding:18px 20px;background:linear-gradient(90deg,{color1},{color2});'>
                    <table role='presentation' width='100%' cellpadding='0' cellspacing='0'>
                      <tr>
                        <td style='color:{dark};font-weight:800;font-size:18px;'>
                          Mi Mundo de Huellitas 🐾
                        </td>
                        <td style='text-align:right;color:{dark};font-weight:700;'>
                          Comprobante #{factura.IdFactura}
                        </td>
                      </tr>
                      <tr>
                        <td colspan='2' style='padding-top:6px;color:rgba(11,15,20,0.85);font-size:13px;font-weight:600;'>
                          Gracias por tu compra — aquí tenés el detalle de tu factura.
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Card body -->
          <tr>
            <td style='background:{card};border-left:1px solid {border};border-right:1px solid {border};padding:22px 20px;'>
              
              <!-- Datos -->
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;'>
                <tr>
                  <td style='padding:0 0 10px 0;color:{dark};font-size:16px;font-weight:800;'>
                    Detalle de la compra
                  </td>
                </tr>
                <tr>
                  <td style='padding:0;color:{muted};font-size:13px;'>
                    <strong style='color:{dark};'>Fecha:</strong> {fecha}<br/>
                    {(string.IsNullOrWhiteSpace(metodoPago) ? "" : $"<strong style='color:{dark};'>Método de pago:</strong> {metodoPago}<br/>")}
                    {(string.IsNullOrWhiteSpace(referencia) ? "" : $"<strong style='color:{dark};'>Referencia:</strong> {referencia}<br/>")}
                  </td>
                </tr>
              </table>

              <!-- Separador -->
              <div style='height:14px;'></div>

              <!-- Tabla productos -->
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border:1px solid {border};border-radius:14px;border-collapse:separate;overflow:hidden;'>
                <tr style='background:#f8fafc;'>
                  <th align='left' style='padding:12px;color:{muted};font-size:12px;text-transform:uppercase;letter-spacing:.06em;'>
                    Producto
                  </th>
                  <th align='center' style='padding:12px;color:{muted};font-size:12px;text-transform:uppercase;letter-spacing:.06em;'>
                    Cant.
                  </th>
                  <th align='right' style='padding:12px;color:{muted};font-size:12px;text-transform:uppercase;letter-spacing:.06em;'>
                    Precio
                  </th>
                  <th align='right' style='padding:12px;color:{muted};font-size:12px;text-transform:uppercase;letter-spacing:.06em;'>
                    Subtotal
                  </th>
                </tr>
                {rows}
                <tr>
                  <td colspan='3' style='padding:14px;border-top:1px solid {border};text-align:right;font-weight:800;color:{dark};'>
                    TOTAL
                  </td>
                  <td style='padding:14px;border-top:1px solid {border};text-align:right;font-weight:900;color:{dark};font-size:16px;'>
                    {factura.MontoTotal.ToString("C")}
                  </td>
                </tr>
              </table>

              {(string.IsNullOrWhiteSpace(obs) ? "" : $@"
              <div style='height:14px;'></div>
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border:1px dashed {border};border-radius:14px;padding:14px;'>
                <tr>
                  <td style='color:{dark};font-weight:800;padding-bottom:6px;'>Observaciones</td>
                </tr>
                <tr>
                  <td style='color:{muted};font-size:13px;line-height:18px;'>{obs}</td>
                </tr>
              </table>
              ")}

              <div style='height:18px;'></div>

              <!-- Mensaje -->
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                  <td style='background:rgba(3,169,244,0.08);border:1px solid rgba(3,169,244,0.18);border-radius:14px;padding:14px;color:{dark};font-size:13px;line-height:18px;'>
                    Si tenés alguna duda, respondé este correo o escribinos a <strong>mundodehuellitascr@outlook.com</strong>.
                    <br/>¡Gracias por confiar en Mi Mundo de Huellitas! 🐶🐱
                  </td>
                </tr>
              </table>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background:{card};border:1px solid {border};border-top:none;border-radius:0 0 18px 18px;padding:16px 20px;text-align:center;color:{muted};font-size:12px;'>
              © {DateTime.Now.Year} Mi Mundo de Huellitas — Este correo es un comprobante automático, por favor no compartas tu información sensible.
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>
</body>
</html>";

            // SMTP config
            string user = ConfigurationManager.AppSettings["smtp.user"];
            string pass = ConfigurationManager.AppSettings["smtp.pass"];
            string host = ConfigurationManager.AppSettings["smtp.host"];
            int port = int.Parse(ConfigurationManager.AppSettings["smtp.port"]);
            bool ssl = bool.Parse(ConfigurationManager.AppSettings["smtp.ssl"]);

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials = new NetworkCredential(user, pass);
                smtp.EnableSsl = ssl;

                var mail = new MailMessage(user, correoDestino);
                mail.Subject = asunto;
                mail.Body = cuerpo;
                mail.IsBodyHtml = true;

                // opcional: mejor compatibilidad
                mail.BodyEncoding = Encoding.UTF8;
                mail.SubjectEncoding = Encoding.UTF8;

                smtp.Send(mail);
            }
        }

        // Helper para evitar que un nombre/observación rompa el HTML
        private static string HtmlSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            return HttpUtility.HtmlEncode(input);
        }
    }
}
