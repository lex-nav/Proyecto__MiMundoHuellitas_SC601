using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteStockVM
    {
        public string Categoria { get; set; }
        public bool? SoloActivos { get; set; }
        public string Texto { get; set; }

        public int TotalProductos { get; set; }
        public int ProductosSinStock { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosStockNormal { get; set; }

        public IEnumerable<SelectListItem> Categorias { get; set; }

        public List<ReporteStockFilaVM> Filas { get; set; } = new List<ReporteStockFilaVM>();
    }

    public class ReporteStockFilaVM
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int StockActual { get; set; }
        public string Estado { get; set; }
        public bool Activo { get; set; }
    }
}