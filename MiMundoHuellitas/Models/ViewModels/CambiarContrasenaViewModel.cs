using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContrasenaActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [Display(Name = "Nueva contraseña")]
        public string NuevaContrasena { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; }
    }
}
