using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class PerfilViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        // FK a dirección (puede ser null)
        public int? IdDireccion { get; set; }

        [Display(Name = "Dirección")]
        public string DireccionDetalle { get; set; }

        [Display(Name = "Código postal")]
        public string CodigoPostal { get; set; }

        public int? IdDistrito { get; set; }

        // opcional, si la usas
        public string Rol { get; set; }
    }
}
