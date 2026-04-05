using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class EmpleadoLaboralVM
    {
        public int IdEmpleadoLaboral { get; set; }
        public int IdEmpleadoJornada { get; set; }
        public int IdUsuario { get; set; }

        [Display(Name = "Empleado")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Salario por hora")]
        public decimal? SalarioHora { get; set; }

        [Display(Name = "Salario mensual")]
        public decimal? SalarioMensual { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [Display(Name = "Jornada")]
        public int IdJornada { get; set; }



        [Display(Name = "Nombre Jornada")]
        public string NombreJornada { get; set; }
    }
}