using System;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class FacturaHistorialVM
    {
        public int IdFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public string Estado { get; set; }
        public string MetodoPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string NumeroReferencia { get; set; }
    }
}