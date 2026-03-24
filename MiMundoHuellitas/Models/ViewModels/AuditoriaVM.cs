using System;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class AuditoriaVM
    {
        public int IdAuditoria { get; set; }
        public string TablaAfectada { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Campo { get; set; }
        public string IdRegistro { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string UsuarioSistema { get; set; }
        public DateTime FechaAuditoria { get; set; }
        public string Observacion { get; set; }
    }
}