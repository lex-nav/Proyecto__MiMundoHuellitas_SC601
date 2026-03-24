using System;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class AuditoriaFiltroVM
    {
        public string Usuario { get; set; }
        public string Modulo { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}