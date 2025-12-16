using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels.Reportes;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity;

namespace MiMundoHuellitas.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly BD_MiMundoHuellitasEntities db = new BD_MiMundoHuellitasEntities();

        // ✅ Menú principal de reportes
        public ActionResult Index()
        {
            return View();
        }

        // ✅ Reporte de Citas (con filtros)
        // GET: /Reportes/Citas?desde=2025-01-01&hasta=2025-01-31&idEstado=2
        public ActionResult Citas(DateTime? desde, DateTime? hasta, int? idEstado)
        {
            var vm = new ReporteCitasVM
            {
                Desde = desde,
                Hasta = hasta,
                IdEstado = idEstado
            };

            // ======================================
            // 1) Cargar dropdown de estados
            // ======================================
            // Si existe TipoEntidad en tu tabla, filtramos por "Cita".
            // Si no existe, igual funciona (carga estados activos).
            var estadosQuery = db.MH_Estado_TB.Where(e => e.Activo);

            // Intentar filtrar por TipoEntidad = "Cita" si la propiedad existe en EF
            // (esto evita que reviente si no tienes esa columna mapeada)
            try
            {
                estadosQuery = estadosQuery.Where(e => e.TipoEntidad == "Cita");
            }
            catch
            {
                // Si tu EF no tiene TipoEntidad, no hacemos nada.
            }

            var estados = estadosQuery.OrderBy(e => e.NombreEstado).ToList();

            vm.EstadosCita.Add(new SelectListItem { Text = "Todos", Value = "" });
            vm.EstadosCita.AddRange(estados.Select(e => new SelectListItem
            {
                Text = e.NombreEstado,
                Value = e.IdEstado.ToString(),
                Selected = idEstado.HasValue && idEstado.Value == e.IdEstado
            }));

            // ======================================
            // 2) Query de citas (incluye relaciones)
            // ======================================
            var q = db.MH_Cita_TB
                .Include(c => c.MH_Estado_TB)
                .Include(c => c.MH_Mascotas_TB)
                .Include(c => c.MH_Usuario_TB)
                .Include(c => c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB))
                .AsQueryable();

            // ======================================
            // 3) Aplicar filtros
            // ======================================
            if (desde.HasValue)
                q = q.Where(x => x.FechaHoraCita >= desde.Value.Date);

            if (hasta.HasValue)
                q = q.Where(x => x.FechaHoraCita < hasta.Value.Date.AddDays(1));

            if (idEstado.HasValue)
                q = q.Where(x => x.IdEstado == idEstado.Value);

            var lista = q
                .OrderByDescending(x => x.FechaHoraCita)
                .ToList();

            // ======================================
            // 4) Mapear a ViewModel para la vista
            // ======================================
            vm.Filas = lista.Select(c => new ReporteCitasRowVM
            {
                IdCita = c.IdCita,
                FechaHora = c.FechaHoraCita,
                Cliente = c.MH_Usuario_TB?.NombreCompleto ?? "(Sin nombre)",
                CorreoCliente = c.MH_Usuario_TB?.Correo ?? "",
                Mascota = c.MH_Mascotas_TB?.NombreMascota ?? "(Sin mascota)",
                Servicios = string.Join(", ",
                    c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB.NombreServicio)),
                Estado = c.MH_Estado_TB?.NombreEstado ?? "(Sin estado)",
                NotasCliente = c.NotasCliente ?? ""
            }).ToList();

            // ======================================
            // 5) Resumen superior
            // ======================================
            vm.Total = vm.Filas.Count;

            var ahora = DateTime.Now;
            vm.Proximas = vm.Filas.Count(x =>
                x.FechaHora >= ahora &&
                !(x.Estado ?? "").ToLower().Contains("cancel"));

            vm.Canceladas = vm.Filas.Count(x =>
                (x.Estado ?? "").ToLower().Contains("cancel"));

            return View(vm);
        }

        // ✅ Exportar CSV del reporte de citas
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
                var servicios = string.Join(" | ",
                    c.MH_Servicios_Cita_TB.Select(sc => sc.MH_Servicios_TB.NombreServicio));

                var notas = (c.NotasCliente ?? "").Replace("\"", "\"\"");

                sb.AppendLine(
                    $"{c.IdCita}," +
                    $"\"{c.FechaHoraCita:yyyy-MM-dd HH:mm}\"," +
                    $"\"{c.MH_Usuario_TB?.NombreCompleto}\"," +
                    $"\"{c.MH_Usuario_TB?.Correo}\"," +
                    $"\"{c.MH_Mascotas_TB?.NombreMascota}\"," +
                    $"\"{servicios}\"," +
                    $"\"{c.MH_Estado_TB?.NombreEstado}\"," +
                    $"\"{notas}\""
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "ReporteCitas.csv");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
