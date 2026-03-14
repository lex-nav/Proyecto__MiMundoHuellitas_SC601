using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class PlanillaDetalleVM
    {
        public long IdPlanilla { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaCalculo { get; set; }

        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }

        public decimal HorasNormales { get; set; }
        public decimal HorasExtra { get; set; }
        public decimal HorasDoble { get; set; }

        public decimal SalarioHora { get; set; }
        public decimal MontoNormales { get; set; }
        public decimal MontoExtra { get; set; }
        public decimal MontoDoble { get; set; }
        public decimal TotalPagar { get; set; }
    }
}
