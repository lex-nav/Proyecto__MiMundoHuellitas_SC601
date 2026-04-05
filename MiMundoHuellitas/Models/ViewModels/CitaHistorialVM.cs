using System;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class CitaHistorialVM
    {
        public int IdCita { get; set; }
        public DateTime FechaHoraCita { get; set; }
        public string Estado { get; set; }
        public string Mascota { get; set; }
        public string Servicio { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal? Subtotal { get; set; }
    }
}