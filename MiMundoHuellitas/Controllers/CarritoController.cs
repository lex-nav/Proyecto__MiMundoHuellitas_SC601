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

namespace MiMundoHuellitas.Controllers
{
    public class CarritoController : Controller
    {
        private BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

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
                // Si guardas el correo en sesión lo puedes precargar:
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

            // 4. Enviar comprobante
            try
            {
                EnviarComprobante(model.CorreoCliente, factura, carrito);
            }
            catch (Exception ex)
            {
                // Para depurar: ver el error en TempData o en el log
                TempData["ErrorCorreo"] = "No se pudo enviar el comprobante: " + ex.Message;
                // NO lanzamos la excepción para no romper la compra
            }


            // 5. Limpiar carrito
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

            return View(factura); // puedes hacer una vista Confirmacion.cshtml
        }

        // ================== CORREO ==================
        private void EnviarComprobante(string correoDestino,
                               MH_Factura_TB factura,
                               List<CarritoItemViewModel> items)
        {
            if (string.IsNullOrWhiteSpace(correoDestino))
                return;

            // 1. Construir cuerpo HTML del comprobante
            var sb = new StringBuilder();

            sb.AppendLine($"Hola,<br><br>");
            sb.AppendLine("Gracias por tu compra en <strong>Mi Mundo de Huellitas</strong> 🐾<br><br>");
            sb.AppendLine($"<strong>Factura N°:</strong> {factura.IdFactura}<br>");
            sb.AppendLine($"<strong>Fecha:</strong> {factura.FechaFactura:dd/MM/yyyy HH:mm}<br>");

            if (!string.IsNullOrWhiteSpace(factura.MetodoPago))
                sb.AppendLine($"<strong>Método de pago:</strong> {factura.MetodoPago}<br>");

            if (!string.IsNullOrWhiteSpace(factura.NumeroReferencia))
                sb.AppendLine($"<strong>Referencia:</strong> {factura.NumeroReferencia}<br>");

            sb.AppendLine("<br>");

            sb.AppendLine("<table border='1' cellspacing='0' cellpadding='6' style='border-collapse:collapse;font-family:Arial;font-size:12px;'>");
            sb.AppendLine("<tr style='background-color:#f5f5f5;font-weight:bold;'>" +
                          "<td>Producto</td><td>Cantidad</td><td>Precio</td><td>Subtotal</td></tr>");

            foreach (var i in items)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{i.NombreProducto}</td>");
                sb.AppendLine($"<td style='text-align:center;'>{i.Cantidad}</td>");
                sb.AppendLine($"<td style='text-align:right;'>{i.PrecioUnitario:C}</td>");
                sb.AppendLine($"<td style='text-align:right;'>{i.Subtotal:C}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<tr style='font-weight:bold;'>");
            sb.AppendLine("<td colspan='3' style='text-align:right;'>TOTAL</td>");
            sb.AppendLine($"<td style='text-align:right;'>{factura.MontoTotal:C}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<br/>");
            sb.AppendLine("<p>¡Gracias por confiar en Mi Mundo de Huellitas! 🐶🐱</p>");

            string asunto = $"Comprobante de compra #{factura.IdFactura} - Mi Mundo de Huellitas";
            string cuerpo = sb.ToString();

            // 2. Leer configuración SMTP igual que en AccountController.EnviarCorreo
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

                smtp.Send(mail);
            }
        }

    }
}
