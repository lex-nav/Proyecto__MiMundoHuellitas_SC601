using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MiMundoHuellitas.Models.ViewModels.Reportes
{
    public class ReporteMascotasVM
    {
        // Filtros
        public int? IdEspecie { get; set; }
        public int? IdRaza { get; set; }
        public bool? SoloActivas { get; set; } // null=todas, true=activas, false=inactivas
        public string Texto { get; set; }      // buscar por nombre mascota o dueño o correo

        // Dropdowns
        public List<SelectListItem> Especies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Razas { get; set; } = new List<SelectListItem>();

        // Resultados
        public List<ReporteMascotasRowVM> Filas { get; set; } = new List<ReporteMascotasRowVM>();

        // Resumen
        public int Total { get; set; }
        public int Activas { get; set; }
        public int Inactivas { get; set; }
    }

    public class ReporteMascotasRowVM
    {
        public int IdMascota { get; set; }
        public string NombreMascota { get; set; }
        public string Especie { get; set; }
        public string Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool Activo { get; set; }
        public string Observaciones { get; set; }

        public string Dueno { get; set; }
        public string CorreoDueno { get; set; }
    }
}
