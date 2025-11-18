using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class RecuperarContrasenaViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Correo")]
        public string Correo { get; set; }
    }
}
