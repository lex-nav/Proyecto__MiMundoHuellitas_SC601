using System.Collections.Generic;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class HistorialClienteViewModel
    {
        public ClienteHistorialVM Cliente { get; set; }
        public List<MascotaHistorialVM> Mascotas { get; set; }
        public List<CitaHistorialVM> Citas { get; set; }
        public List<FacturaHistorialVM> Facturas { get; set; }

        public HistorialClienteViewModel()
        {
            Mascotas = new List<MascotaHistorialVM>();
            Citas = new List<CitaHistorialVM>();
            Facturas = new List<FacturaHistorialVM>();
        }
    }
}