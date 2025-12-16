using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteServiciosVM
    {
        // Filtros
        public string Categoria { get; set; }
        public bool? SoloActivos { get; set; } // null=todos, true=activos, false=inactivos
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }

        // Dropdown categorías
        public List<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();

        // Datos
        public List<ReporteServiciosRowVM> Filas { get; set; } = new List<ReporteServiciosRowVM>();

        // Resumen
        public int Total { get; set; }
        public int Activos { get; set; }
        public int Inactivos { get; set; }
    }

    public class ReporteServiciosRowVM
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public int? DuracionMin { get; set; }
        public bool Activo { get; set; }
        public string Descripcion { get; set; }
    }
}
