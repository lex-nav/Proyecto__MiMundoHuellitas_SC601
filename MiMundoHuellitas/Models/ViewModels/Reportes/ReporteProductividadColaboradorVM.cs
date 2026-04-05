using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteProductividadColaboradorVM
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }

        public int TotalMarcacionesAprobadas { get; set; }
        public decimal HorasTrabajadas { get; set; }

        public int CitasAsignadas { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasConfirmadas { get; set; }
        public int CitasAtendidas { get; set; }
        public int CitasCanceladas { get; set; }
        public int CitasNoAsistio { get; set; }

        public int FacturasRegistradas { get; set; }
        public int FacturasEmitidas { get; set; }
        public int FacturasPagadas { get; set; }
        public int FacturasAnuladas { get; set; }
        public decimal MontoVendido { get; set; }

        public decimal CitasAtendidasPorHora { get; set; }
        public decimal VentaPorHora { get; set; }
    }
}