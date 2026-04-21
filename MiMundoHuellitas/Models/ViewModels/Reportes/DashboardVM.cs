using System;
using System.Collections.Generic;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class DashboardVM
    {
        public decimal VentasMes { get; set; }
        public int CitasHoy { get; set; }
        public int CitasProximas { get; set; }
        public int MascotasActivas { get; set; }
        public int ProductosSinStock { get; set; }
        public int ProductosStockBajo { get; set; }

        public List<string> LabelsVentas { get; set; } = new List<string>();
        public List<decimal> DataVentas { get; set; } = new List<decimal>();

        public List<string> LabelsServicios { get; set; } = new List<string>();
        public List<int> DataServicios { get; set; } = new List<int>();
    }
}