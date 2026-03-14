using System;
using System.Collections.Generic;
using System.Linq;              // ✅ IMPORTANTE para Count(x => ...)
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteMarcacionesVM
    {
        // Filtros
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? IdEmpleado { get; set; }

        // Dropdown empleados
        public List<SelectListItem> Empleados { get; set; } = new List<SelectListItem>();

        // Resultados
        public List<ReporteMarcacionesRowVM> Filas { get; set; } = new List<ReporteMarcacionesRowVM>();

        // Totales
        public int Total => Filas?.Count ?? 0;

        public int Completas => (Filas == null) ? 0 : Filas.Count(x => x.EsCompleta);

        public int Incompletas => (Filas == null) ? 0 : Filas.Count(x => !x.EsCompleta);
    }

    public class ReporteMarcacionesRowVM
    {
        public string Empleado { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }

        public bool EsCompleta => HoraEntrada.HasValue && HoraSalida.HasValue;

        public string Estado => EsCompleta ? "Completo" : "Incompleto";
    }
}
