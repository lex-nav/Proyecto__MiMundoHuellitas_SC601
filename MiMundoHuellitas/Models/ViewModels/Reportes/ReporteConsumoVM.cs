using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteConsumoVM
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string Texto { get; set; }

        public int TotalServiciosConsumidos { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PromedioPorServicio { get; set; }

        public List<ReporteConsumoFilaVM> Filas { get; set; } = new List<ReporteConsumoFilaVM>();
    }

    public class ReporteConsumoFilaVM
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public int VecesConsumido { get; set; }
        public int CantidadTotal { get; set; }
        public decimal IngresoTotal { get; set; }
        public decimal PrecioPromedio { get; set; }
    }
}