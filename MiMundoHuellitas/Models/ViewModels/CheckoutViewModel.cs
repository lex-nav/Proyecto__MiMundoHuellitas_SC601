// Models/CheckoutViewModel.cs
using System.Collections.Generic;

namespace MiMundoHuellitas.Models
{
    public class CheckoutViewModel
    {
        public List<CarritoItemViewModel> Items { get; set; }
        public decimal MontoTotal { get; set; }

        public string MetodoPago { get; set; }
        public string NumeroReferencia { get; set; }
        public string Observaciones { get; set; }
        public string CorreoCliente { get; set; }
    }
}
