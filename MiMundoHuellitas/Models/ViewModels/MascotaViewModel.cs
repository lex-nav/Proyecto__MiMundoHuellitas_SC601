using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class MascotaViewModel
    {
        public int IdMascota { get; set; }

        [Required]
        [Display(Name = "Nombre de la mascota")]
        public string NombreMascota { get; set; }

        [Required]
        [Display(Name = "Especie")]
        public int? IdEspecie { get; set; }   // <-- ahora nullable, pero con Required

        [Required]
        [Display(Name = "Raza")]
        public int? IdRaza { get; set; }      // <-- igual

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }

        public bool Activo { get; set; }

        // Combos
        public IEnumerable<SelectListItem> Especies { get; set; }
        public IEnumerable<SelectListItem> Razas { get; set; }
    }
}
