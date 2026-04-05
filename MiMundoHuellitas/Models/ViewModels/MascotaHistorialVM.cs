using System;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class MascotaHistorialVM
    {
        public int IdMascota { get; set; }
        public string NombreMascota { get; set; }
        public string Especie { get; set; }
        public string Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool Activo { get; set; }
    }
}