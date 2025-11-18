using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Required]
        [Display(Name = "Teléfono")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido de 8 dígitos.")]
        public string Telefono { get; set; }   
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; }
    }
}
