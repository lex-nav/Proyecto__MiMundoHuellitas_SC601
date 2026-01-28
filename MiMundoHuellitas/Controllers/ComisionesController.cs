using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels.Comisiones;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    public class ComisionesController : Controller
    {
        private readonly MiMundoHuellitasEntities db =
            new MiMundoHuellitasEntities();

        // LISTADO
        public ActionResult Index(int? idUsuario, bool? activo)
        {
            var vm = new ComisionesIndexVM();

            vm.Usuarios = new SelectList(
                db.MH_Usuario_TB,
                "IdUsuario",
                "NombreCompleto"
            );

            var query = db.MH_Comision_TB
                          .Include("MH_Usuario_TB")
                          .AsQueryable();

            if (idUsuario.HasValue)
                query = query.Where(x => x.IdUsuario == idUsuario);

            if (activo.HasValue)
                query = query.Where(x => x.Activo == activo);

            vm.Filas = query.Select(x => new ComisionFilaVM
            {
                IdComision = x.IdComision,
                Empleado = x.MH_Usuario_TB.NombreCompleto,
                Tipo = x.TipoComision,
                Monto = x.Porcentaje != null
                    ? x.Porcentaje + "%"
                    : "₡" + x.MontoFijo,
                FechaInicio = x.FechaInicio,
                FechaFin = x.FechaFin,
                Activo = x.Activo ?? false
            }).ToList();

            vm.Total = vm.Filas.Count;
            vm.Activas = vm.Filas.Count(x => x.Activo);
            vm.Inactivas = vm.Filas.Count(x => !x.Activo);

            return View(vm);
        }
        public ActionResult Create()
        {
            var vm = new ComisionFormVM();

            vm.Usuarios = new SelectList(
                db.MH_Usuario_TB,
                "IdUsuario",
                "NombreCompleto"
            );

            vm.FechaInicio = DateTime.Today;
            vm.Activo = true;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ComisionFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Usuarios = new SelectList(
                    db.MH_Usuario_TB,
                    "IdUsuario",
                    "NombreCompleto"
                );

                return View(vm);
            }

            var entidad = new MH_Comision_TB
            {
                IdUsuario = vm.IdUsuario,
                TipoComision = vm.TipoComision,
                Porcentaje = vm.Porcentaje,
                MontoFijo = vm.MontoFijo,
                FechaInicio = vm.FechaInicio,
                FechaFin = vm.FechaFin,
                Activo = vm.Activo,
                FechaRegistro = DateTime.Now
            };

            db.MH_Comision_TB.Add(entidad);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}


