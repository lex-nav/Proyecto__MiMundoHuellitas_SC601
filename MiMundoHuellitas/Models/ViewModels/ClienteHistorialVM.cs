namespace MiMundoHuellitas.Models.ViewModels
{
    public class ClienteHistorialVM
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string TipoUsuario { get; set; }
        public bool Activo { get; set; }
    }
}