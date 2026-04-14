using System;
using System.Collections.Generic;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class PanelMetricasVM
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public decimal VentasPeriodo { get; set; }
        public decimal VentasPeriodoAnterior { get; set; }
        public decimal VariacionVentasPorcentaje { get; set; }

        public int FacturasPeriodo { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal TicketPromedioAnterior { get; set; }
        public decimal VariacionTicketPromedioPorcentaje { get; set; }

        public int CitasHoy { get; set; }
        public int CitasAyer { get; set; }
        public decimal VariacionCitasHoyPorcentaje { get; set; }

        public int CitasProximas { get; set; }
        public int MascotasActivas { get; set; }

        public int ProductosSinStock { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosStockSaludable { get; set; }

        public List<string> VentasMesesLabels { get; set; } = new List<string>();
        public List<decimal> VentasMesesData { get; set; } = new List<decimal>();

        public List<string> CitasEstadoLabels { get; set; } = new List<string>();
        public List<int> CitasEstadoData { get; set; } = new List<int>();

        public List<string> TopServiciosLabels { get; set; } = new List<string>();
        public List<decimal> TopServiciosData { get; set; } = new List<decimal>();

        public List<string> InventarioEstadoLabels { get; set; } = new List<string>();
        public List<int> InventarioEstadoData { get; set; } = new List<int>();

        public List<DashboardUltimaFacturaVM> UltimasFacturas { get; set; } = new List<DashboardUltimaFacturaVM>();
        public List<DashboardProximaCitaVM> ProximasCitasDetalle { get; set; } = new List<DashboardProximaCitaVM>();
        public List<DashboardAlertaInventarioVM> AlertasInventario { get; set; } = new List<DashboardAlertaInventarioVM>();
    }

    public class DashboardUltimaFacturaVM
    {
        public int IdFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public string Cliente { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; }
    }

    public class DashboardProximaCitaVM
    {
        public int IdCita { get; set; }
        public DateTime FechaHora { get; set; }
        public string Cliente { get; set; }
        public string Mascota { get; set; }
        public string Estado { get; set; }
    }

    public class DashboardAlertaInventarioVM
    {
        public string NombreProducto { get; set; }
        public int StockActual { get; set; }
        public string EstadoVisual { get; set; }
    }
}