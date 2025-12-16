using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteFacturasVM
    {
        // filtros
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? IdEstado { get; set; }
        public string MetodoPago { get; set; }
        public string Texto { get; set; } // cliente/correo/#ref

        // dropdown
        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MetodosPago { get; set; } = new List<SelectListItem>();

        // data
        public List<ReporteFacturasRowVM> Filas { get; set; } = new List<ReporteFacturasRowVM>();

        // resumen
        public int TotalFacturas { get; set; }
        public decimal TotalMonto { get; set; }
        public decimal TicketPromedio { get; set; }
    }

    public class ReporteFacturasRowVM
    {
        public int IdFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public string Cliente { get; set; }
        public string CorreoCliente { get; set; }

        public string Estado { get; set; }
        public string MetodoPago { get; set; }
        public string NumeroReferencia { get; set; }

        public int CantidadItems { get; set; }
        public decimal MontoTotal { get; set; }
    }
}
