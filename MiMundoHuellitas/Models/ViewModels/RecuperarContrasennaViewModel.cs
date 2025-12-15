using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class RecuperarContrasenaViewModel
    {
        [Display(Name = "Correo")]
        [Required(ErrorMessage = "Debe completar todos los campos.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debe completar todos los campos.")]
        [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
        public string Nombre { get; set; }
    }
}
