using MiMundoHuellitas.EF;
using System.Collections.Generic;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class HomeProductosViewModel
    {
        public List<MH_Productos_TB> Productos { get; set; } = new List<MH_Productos_TB>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
