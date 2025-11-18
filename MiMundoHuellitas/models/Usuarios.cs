using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Correo { get; set; }

        [Required, StringLength(200)]
        public string ContrasenaHash { get; set; }

        [Required, StringLength(20)]
        public string Rol { get; set; }  
    }
}

