using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class TipoPermisoVM
    {
        public int IdTipoPermiso { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool RequiereDoc { get; set; }
    }

    public class PermisoVM
    {
        public long IdPermiso { get; set; }
        public int IdUsuario { get; set; }
        public string Empleado { get; set; }
        public string TipoPermiso { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int DiasSolicitados { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string ComentarioAdmin { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string AdminResolvio { get; set; }
    }

    public class SolicitarPermisoVM
    {
        [Required(ErrorMessage = "Seleccione el tipo de permiso.")]
        [Display(Name = "Tipo de permiso")]
        public int IdTipoPermiso { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha de inicio.")]
        [Display(Name = "Fecha inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Ingrese la fecha de fin.")]
        [Display(Name = "Fecha fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Ingrese el motivo.")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; }

        public List<TipoPermisoVM> TiposPermiso { get; set; }
    }

    public class ResolverPermisoVM
    {
        [Required]
        public long IdPermiso { get; set; }

        [Required(ErrorMessage = "Seleccione una decisión.")]
        public string NuevoEstado { get; set; }  // "Aprobado" | "Rechazado"

        [StringLength(500)]
        public string ComentarioAdmin { get; set; }

        // Solo para mostrar en el modal
        public string Empleado { get; set; }
        public string TipoPermiso { get; set; }
        public string FechasRango { get; set; }
        public string Motivo { get; set; }
    }

    public class AdminPermisosVM
    {
        public string FiltroEstado { get; set; }
        public List<PermisoVM> Permisos { get; set; }
        public ResolverPermisoVM Resolver { get; set; }
    }
}
