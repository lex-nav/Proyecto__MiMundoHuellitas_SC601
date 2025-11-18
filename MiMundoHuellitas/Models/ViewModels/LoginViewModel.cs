using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        public bool Recordarme { get; set; }
    }
}
