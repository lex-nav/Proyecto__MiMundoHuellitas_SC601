using System.Collections.Generic;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class ProductosListadoViewModel
    {
        public List<ProductoItemViewModel> Productos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class ProductoItemViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int StockActual { get; set; }
        public string ImagenUrl { get; set; }
    }
}
