using System;
using System.Collections.Generic;
using System.Web.Mvc;


namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteAuditoriaVM
    {
        // Filtros
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public int? IdUsuarioAfectado { get; set; }

        // Dropdown empleados (o usuarios) afectados
        public List<SelectListItem> Usuarios { get; set; } = new List<SelectListItem>();

        // Resultados
        public List<ReporteAuditoriaRowVM> Filas { get; set; } = new List<ReporteAuditoriaRowVM>();
    }


    public class ReporteAuditoriaRowVM
    {
        public int IdAuditoria { get; set; }
        public DateTime Fecha { get; set; }

        public string Admin { get; set; }
        public string UsuarioAfectado { get; set; }

        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Campo { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string Observacion { get; set; }
    }
}