using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using MiMundoHuellitas.Models.ViewModels.Reportes;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;
<<<<<<< HEAD
using System.Data.Entity;
using System.Collections.Generic;
=======
>>>>>>> Sebas

namespace MiMundoHuellitas.Controllers
{
    // 🔐 SOLO ADMIN: todo lo que está en /Reportes es exclusivamente para administradores.
    // Requiere que el rol se cargue en Application_AuthenticateRequest (Global.asax) desde el FormsAuth ticket.
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // ============================
        // MENÚ PRINCIPAL DE REPORTES
        // ============================
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Reportes";
            return View();
        }

        // ============================
        // REPORTE DE MARCACIONES (EMPLEADOS)
        // ============================
        public ActionResult Marcaciones(DateTime? desde, DateTime? hasta, int? idEmpleado)
        {
            ViewBag.ActiveMenu = "Reportes";

            var vm = new ReporteMarcacionesVM
            {
                Desde = desde,
                Hasta = hasta,
                IdEmpleado = idEmpleado
            };

            // Dropdown empleados (solo tipo empleado = 3)
            var empleados = db.MH_Usuario_TB
                .Where(u => u.Activo && u.IdTipoUsuario == 3)
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = u.NombreCompleto,
                    Selected = (idEmpleado.HasValue && u.IdUsuario == idEmpleado.Value)
                })
                .ToList();

            empleados.Insert(0, new SelectListItem { Text = "Todos", Value = "" });
            vm.Empleados = empleados;

            // Query marcaciones
            var q = db.MH_Marcacion_TB
                .Include(m => m.MH_Usuario_TB)
                .AsQueryable();

            if (desde.HasValue)
            {
                DateTime d = new DateTime(desde.Value.Year, desde.Value.Month, desde.Value.Day);
                q = q.Where(x => x.Fecha >= d);
            }

            if (hasta.HasValue)
            {
                DateTime h = new DateTime(hasta.Value.Year, hasta.Value.Month, hasta.Value.Day).AddDays(1);
                q = q.Where(x => x.Fecha < h);
            }


            if (idEmpleado.HasValue)
                q = q.Where(x => x.IdUsuario == idEmpleado.Value);

            var data = q
                .OrderByDescending(x => x.Fecha)
                .ToList();

            vm.Filas = data.Select(x => new ReporteMarcacionesRowVM
            {
                Empleado = x.MH_Usuario_TB?.NombreCompleto ?? "(Sin nombre)",
                Fecha = x.Fecha,
                HoraEntrada = x.HoraEntrada,
                HoraSalida = x.HoraSalida
            }).ToList();

            return View(vm);
        }

        public ActionResult Auditoria(DateTime? desde, DateTime? hasta, string modulo, string accion, int? idUsuarioAfectado)
        {
            ViewBag.ActiveMenu = "Reportes";

            var vm = new ReporteAuditoriaVM
            {
                Desde = desde,
                Hasta = hasta,
                Modulo = (modulo ?? "").Trim(),
                Accion = (accion ?? "").Trim(),
                IdUsuarioAfectado = idUsuarioAfectado
            };

            // Dropdown: usuarios afectados (puedes filtrar solo empleados si quieres IdTipoUsuario==3)
            var usuarios = db.MH_Usuario_TB
                .Where(u => u.Activo)
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = u.NombreCompleto,
                    Selected = (idUsuarioAfectado.HasValue && u.IdUsuario == idUsuarioAfectado.Value)
                })
                .ToList();

            usuarios.Insert(0, new SelectListItem { Text = "Todos", Value = "" });
            vm.Usuarios = usuarios;

            // Query auditoría
            // Temporalmente se comenta porque la entidad MH_Auditoria_TB
            // no existe en el modelo EF actual.
            //var q = db.MH_Auditoria_TB
            //  .Include(a => a.MH_Usuario_TB)   // Admin (según tu modelo)
            //  .Include(a => a.MH_Usuario_TB1)  // Usuario afectado (según tu modelo)
            //.AsQueryable();

            //if (desde.HasValue)
            //    q = q.Where(x => x.Fecha >= desde.Value.Date);

            //if (hasta.HasValue)
            //    q = q.Where(x => x.Fecha < hasta.Value.Date.AddDays(1));

            //if (!string.IsNullOrWhiteSpace(vm.Modulo))
            //    q = q.Where(x => x.Modulo.Contains(vm.Modulo));

            //if (!string.IsNullOrWhiteSpace(vm.Accion))
            //    q = q.Where(x => x.Accion.Contains(vm.Accion));

            //if (idUsuarioAfectado.HasValue)
            //    q = q.Where(x => x.IdUsuarioAfectado == idUsuarioAfectado.Value);

            //var data = q
            //    .OrderByDescending(x => x.Fecha)
            //    .ToList();

            //vm.Filas = data.Select(x => new ReporteAuditoriaRowVM
            //{
            //    IdAuditoria = x.IdAuditoria,
            //    Fecha = x.Fecha,
            //    Admin = x.MH_Usuario_TB?.NombreCompleto ?? "(Sin admin)",
            //    UsuarioAfectado = x.MH_Usuario_TB1?.NombreCompleto ?? "(Sin usuario)",
            //    Modulo = x.Modulo ?? "",
            //    Accion = x.Accion ?? "",
            //    Campo = x.Campo ?? "",
            //    ValorAnterior = x.ValorAnterior ?? "",
            //    ValorNuevo = x.ValorNuevo ?? "",
            //    Observacion = x.Observacion ?? ""
            //}).ToList();
            vm.Filas = new List<ReporteAuditoriaRowVM>();
            return View(vm);
        }



        // REPORTE DE CITAS
        // ============================
        public ActionResult Citas(DateTime? desde, DateTime? hasta, int? idEstado)
        {
            ViewBag.ActiveMenu = "Reportes";

            var vm = new ReporteCitasVM
            {
                Desde = desde,
                Hasta = hasta,
                IdEstado = idEstado
            };

            // Estados (dropdown)
            var estadosQuery = db.MH_Estado_TB.Where(e => e.Activo);
            try { estadosQuery = estadosQuery.Where(e => e.TipoEntidad == "Cita"); } catch { }

            var estados = estadosQuery.OrderBy(e => e.NombreEstado).ToList();

            vm.EstadosCita.Add(new SelectListItem { Text = "Todos", Value = "" });
            vm.EstadosCita.AddRange(estados.Select(e => new SelectListItem
            {
                Text = e.NombreEstado,
                Value = e.IdEstado.ToString(),
                Selected = idEstado.HasValue && idEstado.Value == e.IdEstado
            }));

            // Query
            var q = db.MH_Cita_TB
                .Include(c => c.MH_Estado_TB)
                .Include(c => c.MH_Mascotas_TB)
                .Include(c => c.MH_Usuario_TB)
                .Include(c => c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB))
                .AsQueryable();

            if (desde.HasValue)
                q = q.Where(x => x.FechaHoraCita >= desde.Value.Date);

            if (hasta.HasValue)
                q = q.Where(x => x.FechaHoraCita < hasta.Value.Date.AddDays(1));

            if (idEstado.HasValue)
                q = q.Where(x => x.IdEstado == idEstado.Value);

            var lista = q.OrderByDescending(x => x.FechaHoraCita).ToList();

            vm.Filas = lista.Select(c => new ReporteCitasRowVM
            {
                IdCita = c.IdCita,
                FechaHora = c.FechaHoraCita,
                Cliente = c.MH_Usuario_TB?.NombreCompleto ?? "(Sin nombre)",
                CorreoCliente = c.MH_Usuario_TB?.Correo ?? "",
                Mascota = c.MH_Mascotas_TB?.NombreMascota ?? "(Sin mascota)",
                Servicios = string.Join(", ", c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB.NombreServicio)),
                Estado = c.MH_Estado_TB?.NombreEstado ?? "(Sin estado)",
                NotasCliente = c.NotasCliente ?? ""
            }).ToList();

            vm.Total = vm.Filas.Count;

            var ahora = DateTime.Now;
            vm.Proximas = vm.Filas.Count(x => x.FechaHora >= ahora && !(x.Estado ?? "").ToLower().Contains("cancel"));
            vm.Canceladas = vm.Filas.Count(x => (x.Estado ?? "").ToLower().Contains("cancel"));

            return View(vm);
        }

        // ============================
        // EXPORTAR CSV (CITAS)
        // ============================
        public FileResult ExportCitasCsv(DateTime? desde, DateTime? hasta, int? idEstado)
        {
            var q = db.MH_Cita_TB
                .Include(c => c.MH_Estado_TB)
                .Include(c => c.MH_Mascotas_TB)
                .Include(c => c.MH_Usuario_TB)
                .Include(c => c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB))
                .AsQueryable();

            if (desde.HasValue)
                q = q.Where(x => x.FechaHoraCita >= desde.Value.Date);

            if (hasta.HasValue)
                q = q.Where(x => x.FechaHoraCita < hasta.Value.Date.AddDays(1));

            if (idEstado.HasValue)
                q = q.Where(x => x.IdEstado == idEstado.Value);

            var data = q.OrderByDescending(x => x.FechaHoraCita).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("IdCita,FechaHora,Cliente,Correo,Mascota,Servicios,Estado,Notas");

            foreach (var c in data)
            {
                var servicios = string.Join(" | ", c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB.NombreServicio));
                var notas = (c.NotasCliente ?? "").Replace("\"", "\"\"");

                sb.AppendLine(
                    $"{c.IdCita}," +
                    $"\"{c.FechaHoraCita:yyyy-MM-dd HH:mm}\"," +
                    $"\"{(c.MH_Usuario_TB?.NombreCompleto ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(c.MH_Usuario_TB?.Correo ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(c.MH_Mascotas_TB?.NombreMascota ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{servicios.Replace("\"", "\"\"")}\"," +
                    $"\"{(c.MH_Estado_TB?.NombreEstado ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{notas}\""
                );
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "ReporteCitas.csv");
        }

        // ============================
        // REPORTE DE SERVICIOS
        // ============================
        public ActionResult Servicios(string categoria, bool? soloActivos, decimal? precioMin, decimal? precioMax)
        {
            ViewBag.ActiveMenu = "Reportes";

            var vm = new ReporteServiciosVM
            {
                Categoria = (categoria ?? "").Trim(),
                SoloActivos = soloActivos,
                PrecioMin = precioMin,
                PrecioMax = precioMax
            };

            var cats = db.MH_Servicios_TB
                .Select(s => s.Categoria)
                .Distinct()
                .ToList()
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .OrderBy(c => c)
                .ToList();

            vm.Categorias.Add(new SelectListItem { Text = "Todas", Value = "" });
            vm.Categorias.AddRange(cats.Select(c => new SelectListItem
            {
                Text = c,
                Value = c,
                Selected = (!string.IsNullOrWhiteSpace(vm.Categoria) &&
                            vm.Categoria.Equals(c, StringComparison.OrdinalIgnoreCase))
            }));

            var q = db.MH_Servicios_TB.AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.Categoria))
                q = q.Where(x => x.Categoria == vm.Categoria);

            if (soloActivos.HasValue)
                q = q.Where(x => x.Activo == soloActivos.Value);

            if (precioMin.HasValue)
                q = q.Where(x => x.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                q = q.Where(x => x.Precio <= precioMax.Value);

            var lista = q.OrderBy(x => x.NombreServicio).ToList();

            vm.Filas = lista.Select(s => new ReporteServiciosRowVM
            {
                IdServicio = s.IdServicio,
                NombreServicio = s.NombreServicio,
                Categoria = s.Categoria ?? "",
                Precio = s.Precio,
                DuracionMin = s.DuracionMin,
                Activo = s.Activo,
                Descripcion = s.Descripcion ?? ""
            }).ToList();

            vm.Total = vm.Filas.Count;
            vm.Activos = vm.Filas.Count(x => x.Activo);
            vm.Inactivos = vm.Filas.Count(x => !x.Activo);

            return View(vm);
        }

        // ============================
        // EXPORTAR CSV (SERVICIOS)
        // ============================
        public FileResult ExportServiciosCsv(string categoria, bool? soloActivos, decimal? precioMin, decimal? precioMax)
        {
            categoria = (categoria ?? "").Trim();

            var q = db.MH_Servicios_TB.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                q = q.Where(x => x.Categoria == categoria);

            if (soloActivos.HasValue)
                q = q.Where(x => x.Activo == soloActivos.Value);

            if (precioMin.HasValue)
                q = q.Where(x => x.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                q = q.Where(x => x.Precio <= precioMax.Value);

            var data = q.OrderBy(x => x.NombreServicio).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("IdServicio,NombreServicio,Categoria,Precio,DuracionMin,Activo,Descripcion");

            foreach (var s in data)
            {
                string desc = (s.Descripcion ?? "").Replace("\"", "\"\"");
                sb.AppendLine(
                    $"{s.IdServicio}," +
                    $"\"{(s.NombreServicio ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(s.Categoria ?? "").Replace("\"", "\"\"")}\"," +
                    $"{s.Precio}," +
                    $"{(s.DuracionMin.HasValue ? s.DuracionMin.Value.ToString() : "")}," +
                    $"{(s.Activo ? "Si" : "No")}," +
                    $"\"{desc}\""
                );
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "ReporteServicios.csv");
        }

        // ============================
        // REPORTE DE MASCOTAS
        // ============================
        public ActionResult Mascotas(int? idEspecie, int? idRaza, bool? soloActivas, string texto)
        {
            ViewBag.ActiveMenu = "Reportes";

            var vm = new ReporteMascotasVM
            {
                IdEspecie = idEspecie,
                IdRaza = idRaza,
                SoloActivas = soloActivas,
                Texto = (texto ?? "").Trim()
            };

            var especies = db.MH_Especie_TB.OrderBy(e => e.NombreEspecie).ToList();
            vm.Especies.Add(new SelectListItem { Text = "Todas", Value = "" });
            vm.Especies.AddRange(especies.Select(e => new SelectListItem
            {
                Text = e.NombreEspecie,
                Value = e.IdEspecie.ToString(),
                Selected = idEspecie.HasValue && e.IdEspecie == idEspecie.Value
            }));

            var razasQuery = db.MH_Raza_TB.AsQueryable();
            if (idEspecie.HasValue)
                razasQuery = razasQuery.Where(r => r.IdEspecie == idEspecie.Value);

            var razas = razasQuery.OrderBy(r => r.NombreRaza).ToList();
            vm.Razas.Add(new SelectListItem { Text = "Todas", Value = "" });
            vm.Razas.AddRange(razas.Select(r => new SelectListItem
            {
                Text = r.NombreRaza,
                Value = r.IdRaza.ToString(),
                Selected = idRaza.HasValue && r.IdRaza == idRaza.Value
            }));

            var q = db.MH_Mascotas_TB
                .Include(m => m.MH_Usuario_TB)
                .Include(m => m.MH_Especie_TB)
                .Include(m => m.MH_Raza_TB)
                .AsQueryable();

            if (idEspecie.HasValue)
                q = q.Where(m => m.IdEspecie == idEspecie.Value);

            if (idRaza.HasValue)
                q = q.Where(m => m.IdRaza == idRaza.Value);

            if (soloActivas.HasValue)
                q = q.Where(m => m.Activo == soloActivas.Value);

            if (!string.IsNullOrWhiteSpace(vm.Texto))
            {
                string t = vm.Texto;
                q = q.Where(m =>
                    m.NombreMascota.Contains(t) ||
                    m.MH_Usuario_TB.NombreCompleto.Contains(t) ||
                    m.MH_Usuario_TB.Correo.Contains(t));
            }

            var lista = q.OrderBy(m => m.NombreMascota).ToList();

            vm.Filas = lista.Select(m => new ReporteMascotasRowVM
            {
                IdMascota = m.IdMascota,
                NombreMascota = m.NombreMascota,
                Especie = m.MH_Especie_TB?.NombreEspecie ?? "",
                Raza = m.MH_Raza_TB?.NombreRaza ?? "",
                FechaNacimiento = m.FechaNacimiento,
                Observaciones = m.Observaciones ?? "",
                Activo = m.Activo,
                Dueno = m.MH_Usuario_TB?.NombreCompleto ?? "",
                CorreoDueno = m.MH_Usuario_TB?.Correo ?? ""
            }).ToList();

            vm.Total = vm.Filas.Count;
            vm.Activas = vm.Filas.Count(x => x.Activo);
            vm.Inactivas = vm.Filas.Count(x => !x.Activo);

            return View(vm);
        }

        // ============================
        // EXPORTAR CSV (MASCOTAS)
        // ============================
        public FileResult ExportMascotasCsv(int? idEspecie, int? idRaza, bool? soloActivas, string texto)
        {
            texto = (texto ?? "").Trim();

            var q = db.MH_Mascotas_TB
                .Include(m => m.MH_Usuario_TB)
                .Include(m => m.MH_Especie_TB)
                .Include(m => m.MH_Raza_TB)
                .AsQueryable();

            if (idEspecie.HasValue)
                q = q.Where(m => m.IdEspecie == idEspecie.Value);

            if (idRaza.HasValue)
                q = q.Where(m => m.IdRaza == idRaza.Value);

            if (soloActivas.HasValue)
                q = q.Where(m => m.Activo == soloActivas.Value);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                string t = texto;
                q = q.Where(m =>
                    m.NombreMascota.Contains(t) ||
                    m.MH_Usuario_TB.NombreCompleto.Contains(t) ||
                    m.MH_Usuario_TB.Correo.Contains(t));
            }

            var data = q.OrderBy(m => m.NombreMascota).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("IdMascota,NombreMascota,Especie,Raza,FechaNacimiento,Activo,Dueno,CorreoDueno,Observaciones");

            foreach (var m in data)
            {
                string obs = (m.Observaciones ?? "").Replace("\"", "\"\"");
                sb.AppendLine(
                    $"{m.IdMascota}," +
                    $"\"{(m.NombreMascota ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(m.MH_Especie_TB?.NombreEspecie ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(m.MH_Raza_TB?.NombreRaza ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(m.FechaNacimiento.HasValue ? m.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "")}\"," +
                    $"{(m.Activo ? "Si" : "No")}," +
                    $"\"{(m.MH_Usuario_TB?.NombreCompleto ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{(m.MH_Usuario_TB?.Correo ?? "").Replace("\"", "\"\"")}\"," +
                    $"\"{obs}\""
                );
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "ReporteMascotas.csv");
        }

        // ============================
        // REPORTE DE VENTAS (FACTURAS)
        // ============================
        public ActionResult Facturas(DateTime? desde, DateTime? hasta, int? idEstado, string metodoPago, string texto)
        {
            ViewBag.ActiveMenu = "Reportes";

            var vm = new ReporteFacturasVM
            {
                Desde = desde,
                Hasta = hasta,
                IdEstado = idEstado,
                MetodoPago = (metodoPago ?? "").Trim(),
                Texto = (texto ?? "").Trim()
            };

            vm.Estados.Add(new SelectListItem { Text = "Todos", Value = "" });

            var estados = db.MH_Estado_TB
                .Where(e => e.Activo && e.TipoEntidad == "Factura")
                .OrderBy(e => e.NombreEstado)
                .ToList();

            vm.Estados.AddRange(estados.Select(e => new SelectListItem
            {
                Text = e.NombreEstado,
                Value = e.IdEstado.ToString(),
                Selected = idEstado.HasValue && idEstado.Value == e.IdEstado
            }));

            vm.MetodosPago.Add(new SelectListItem { Text = "Todos", Value = "" });

            var metodos = db.MH_Factura_TB
                .Select(f => f.MetodoPago)
                .Distinct()
                .ToList()
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .OrderBy(m => m)
                .ToList();

            vm.MetodosPago.AddRange(metodos.Select(m => new SelectListItem
            {
                Text = m,
                Value = m,
                Selected = (!string.IsNullOrWhiteSpace(vm.MetodoPago) &&
                            vm.MetodoPago.Equals(m, StringComparison.OrdinalIgnoreCase))
            }));

            var q = db.MH_Factura_TB
                .Include(f => f.MH_Usuario_TB)
                .Include(f => f.MH_Estado_TB)
                .Include(f => f.MH_DetalleFactura_TB)
                .AsQueryable();

            if (desde.HasValue)
                q = q.Where(f => f.FechaFactura >= desde.Value.Date);

            if (hasta.HasValue)
                q = q.Where(f => f.FechaFactura < hasta.Value.Date.AddDays(1));

            if (idEstado.HasValue)
                q = q.Where(f => f.IdEstado == idEstado.Value);

            if (!string.IsNullOrWhiteSpace(vm.MetodoPago))
                q = q.Where(f => f.MetodoPago == vm.MetodoPago);

            if (!string.IsNullOrWhiteSpace(vm.Texto))
            {
                string t = vm.Texto;
                q = q.Where(f =>
                    f.MH_Usuario_TB.NombreCompleto.Contains(t) ||
                    f.MH_Usuario_TB.Correo.Contains(t) ||
                    (f.NumeroReferencia ?? "").Contains(t)
                );
            }

            var lista = q.OrderByDescending(f => f.FechaFactura).ToList();

            vm.Filas = lista.Select(f => new ReporteFacturasRowVM
            {
                IdFactura = f.IdFactura,
                FechaFactura = f.FechaFactura,
                Cliente = f.MH_Usuario_TB?.NombreCompleto ?? "",
                CorreoCliente = f.MH_Usuario_TB?.Correo ?? "",
                Estado = f.MH_Estado_TB?.NombreEstado ?? "",
                MetodoPago = f.MetodoPago ?? "",
                NumeroReferencia = f.NumeroReferencia ?? "",
                CantidadItems = f.MH_DetalleFactura_TB?.Sum(d => d.Cantidad) ?? 0,
                MontoTotal = f.MontoTotal
            }).ToList();

            vm.TotalFacturas = vm.Filas.Count;
            vm.TotalMonto = vm.Filas.Sum(x => x.MontoTotal);
            vm.TicketPromedio = (vm.TotalFacturas > 0) ? (vm.TotalMonto / vm.TotalFacturas) : 0;

            return View(vm);
        }

        // ============================
        // EXPORTAR CSV (FACTURAS)
        // ============================
        public FileResult ExportFacturasCsv(DateTime? desde, DateTime? hasta, int? idEstado, string metodoPago, string texto)
        {
            metodoPago = (metodoPago ?? "").Trim();
            texto = (texto ?? "").Trim();

            var q = db.MH_Factura_TB
                .Include(f => f.MH_Usuario_TB)
                .Include(f => f.MH_Estado_TB)
                .Include(f => f.MH_DetalleFactura_TB)
                .AsQueryable();

            if (desde.HasValue)
                q = q.Where(f => f.FechaFactura >= desde.Value.Date);

            if (hasta.HasValue)
                q = q.Where(f => f.FechaFactura < hasta.Value.Date.AddDays(1));

            if (idEstado.HasValue)
                q = q.Where(f => f.IdEstado == idEstado.Value);

            if (!string.IsNullOrWhiteSpace(metodoPago))
                q = q.Where(f => f.MetodoPago == metodoPago);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                string t = texto;
                q = q.Where(f =>
                    f.MH_Usuario_TB.NombreCompleto.Contains(t) ||
                    f.MH_Usuario_TB.Correo.Contains(t) ||
                    (f.NumeroReferencia ?? "").Contains(t)
                );
            }

            var data = q.OrderByDescending(f => f.FechaFactura).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("IdFactura,FechaFactura,Cliente,Correo,Estado,MetodoPago,NumeroReferencia,CantidadItems,MontoTotal,Observaciones");

            foreach (var f in data)
            {
                var cliente = (f.MH_Usuario_TB?.NombreCompleto ?? "").Replace("\"", "\"\"");
                var correo = (f.MH_Usuario_TB?.Correo ?? "").Replace("\"", "\"\"");
                var estado = (f.MH_Estado_TB?.NombreEstado ?? "").Replace("\"", "\"\"");
                var mp = (f.MetodoPago ?? "").Replace("\"", "\"\"");
                var refn = (f.NumeroReferencia ?? "").Replace("\"", "\"\"");
                var obs = (f.Observaciones ?? "").Replace("\"", "\"\"");

                int items = f.MH_DetalleFactura_TB?.Sum(d => d.Cantidad) ?? 0;

                sb.AppendLine(
                    $"{f.IdFactura}," +
                    $"\"{f.FechaFactura:yyyy-MM-dd HH:mm}\"," +
                    $"\"{cliente}\"," +
                    $"\"{correo}\"," +
                    $"\"{estado}\"," +
                    $"\"{mp}\"," +
                    $"\"{refn}\"," +
                    $"{items}," +
                    $"{f.MontoTotal}," +
                    $"\"{obs}\""
                );
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "ReporteFacturas.csv");
        }
        
        //PRODUCTIVIDAD POR COLABORADOR
        
        public ActionResult ProductividadColaborador(DateTime? desde, DateTime? hasta)
        {
            ViewBag.ActiveMenu = "Reportes";

            if (!desde.HasValue)
                desde = DateTime.Today.AddDays(-30);

            if (!hasta.HasValue)
                hasta = DateTime.Today;

            var pDesde = new SqlParameter("@FechaInicio", desde.Value.Date);
            var pHasta = new SqlParameter("@FechaFin", hasta.Value.Date);

            var data = db.Database.SqlQuery<ReporteProductividadColaboradorVM>(
                "EXEC dbo.sp_ReporteProductividadColaborador @FechaInicio, @FechaFin",
                pDesde, pHasta
            ).ToList();

            ViewBag.Desde = desde.Value.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta.Value.ToString("yyyy-MM-dd");

            return View(data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
