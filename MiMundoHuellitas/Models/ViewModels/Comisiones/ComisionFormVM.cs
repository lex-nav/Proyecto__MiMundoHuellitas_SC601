using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Comisiones
{
    public class ComisionFormVM
    {
        public int IdComision { get; set; }

        [Required(ErrorMessage = "Seleccione un empleado")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Indique el tipo")]
        public string TipoComision { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje debe ser entre 0 y 100")]
        public decimal? Porcentaje { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monto inválido")]
        public decimal? MontoFijo { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public bool Activo { get; set; }

        // Para combo
        public SelectList Usuarios { get; set; }
    }
}