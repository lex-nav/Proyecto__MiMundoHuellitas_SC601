using System;

namespace MiMundoHuellitas.Models
{
    public class Cita
    {
        public int IdCita { get; set; }

        // Llaves foráneas
        public int IdUsuario { get; set; }
        public int IdMascota { get; set; }
        public int IdEstado { get; set; }   // Pendiente / Confirmada / Cancelada
        public DateTime FechaHoraCita { get; set; }
        public string NotasCliente { get; set; }
        public string NotasInternas { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualiza { get; set; }
    }
}