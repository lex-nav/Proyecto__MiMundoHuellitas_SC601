// Models/CarritoItemViewModel.cs
using System;

namespace MiMundoHuellitas.Models
{
    public class CarritoItemViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }

        public decimal Subtotal
        {
            get { return PrecioUnitario * Cantidad; }
        }
    }
}
