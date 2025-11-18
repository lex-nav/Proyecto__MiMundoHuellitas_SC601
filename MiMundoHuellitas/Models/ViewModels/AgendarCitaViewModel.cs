using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class AgendarCitaViewModel
    {
        public int IdCita { get; set; }

        [Required]
        [Display(Name = "Mascota")]
        public int? IdMascota { get; set; }

        [Required]
        [Display(Name = "Servicio")]
        public int? IdServicio { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime? Fecha { get; set; }

        [Required]
        [Display(Name = "Hora")]
        public string Hora { get; set; } 

        [Display(Name = "Notas del cliente")]
        public string NotasCliente { get; set; }

        public IEnumerable<SelectListItem> Mascotas { get; set; }
        public IEnumerable<SelectListItem> Servicios { get; set; }
        public IEnumerable<SelectListItem> Horarios { get; set; }
    }
}
