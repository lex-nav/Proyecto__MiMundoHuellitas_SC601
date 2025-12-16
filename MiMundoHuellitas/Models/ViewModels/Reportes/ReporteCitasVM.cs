using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteCitasRowVM
    {
        public int IdCita { get; set; }
        public DateTime FechaHora { get; set; }
        public string Cliente { get; set; }
        public string CorreoCliente { get; set; }
        public string Mascota { get; set; }
        public string Servicios { get; set; }
        public string Estado { get; set; }
        public string NotasCliente { get; set; }
    }

    public class ReporteCitasVM
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? IdEstado { get; set; }

        public List<SelectListItem> EstadosCita { get; set; } = new List<SelectListItem>();
        public List<ReporteCitasRowVM> Filas { get; set; } = new List<ReporteCitasRowVM>();

        public int Total { get; set; }
        public int Proximas { get; set; }
        public int Canceladas { get; set; }
    }
}
