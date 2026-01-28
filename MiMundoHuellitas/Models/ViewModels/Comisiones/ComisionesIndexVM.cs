using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Comisiones
{
    public class ComisionesIndexVM
    {
        public int? IdUsuario { get; set; }
        public bool? Activo { get; set; }

        public SelectList Usuarios { get; set; }

        public List<ComisionFilaVM> Filas { get; set; }

        public int Total { get; set; }
        public int Activas { get; set; }
        public int Inactivas { get; set; }
    }

    public class ComisionFilaVM
    {
        public int IdComision { get; set; }
        public string Empleado { get; set; }
        public string Tipo { get; set; }
        public string Monto { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool Activo { get; set; }
    }
}