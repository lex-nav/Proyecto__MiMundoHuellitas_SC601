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

        // NUEVA COMISION
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

            // Regla de negocio: debe haber porcentaje o monto
            if (vm.Porcentaje == null && vm.MontoFijo == null)
            {
                ModelState.AddModelError("",
                    "Debe indicar un porcentaje o un monto fijo.");
            }

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

        // EDITAR COMISION 

        // GET: Edit
        public ActionResult Edit(int? id)
        {

            if (id == null)
                return RedirectToAction("Index");

            var entidad = db.MH_Comision_TB.Find(id);

            if (entidad == null)
                return HttpNotFound();

            var vm = new ComisionFormVM
            {
                IdComision = entidad.IdComision,
                IdUsuario = entidad.IdUsuario,
                TipoComision = entidad.TipoComision,
                Porcentaje = entidad.Porcentaje,
                MontoFijo = entidad.MontoFijo,
                FechaInicio = entidad.FechaInicio,
                FechaFin = entidad.FechaFin,
                Activo = entidad.Activo ?? false
            };

            vm.Usuarios = new SelectList(
                db.MH_Usuario_TB,
                "IdUsuario",
                "NombreCompleto",
                vm.IdUsuario
            );

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ComisionFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Usuarios = new SelectList(
                    db.MH_Usuario_TB,
                    "IdUsuario",
                    "NombreCompleto",
                    vm.IdUsuario
                );

                return View(vm);
            }

            var entidad = db.MH_Comision_TB.Find(vm.IdComision);

            if (entidad == null)
                return HttpNotFound();

            entidad.IdUsuario = vm.IdUsuario;
            entidad.TipoComision = vm.TipoComision;
            entidad.Porcentaje = vm.Porcentaje;
            entidad.MontoFijo = vm.MontoFijo;
            entidad.FechaInicio = vm.FechaInicio;
            entidad.FechaFin = vm.FechaFin;
            entidad.Activo = vm.Activo;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ELIMINAR COMISION
        public ActionResult Delete(int id)
        {
            var comision = db.MH_Comision_TB.Find(id);

            if (comision == null)
                return HttpNotFound();

            db.MH_Comision_TB.Remove(comision);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}


