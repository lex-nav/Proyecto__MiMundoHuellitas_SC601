using MiMundoHuellitas.DAL;
using MiMundoHuellitas.Models.ViewModels;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class PermisoController : Controller
    {
        private readonly PermisoRepository _repo = new PermisoRepository();

        // Lee directo de Session igual que el resto de tus controllers
        private int IdUsuario => (int)Session["IdUsuario"];
        private bool EsAdmin => User.IsInRole("Admin");
        private bool EsEmpleado => User.IsInRole("Empleado");

        // ============================================================
        //  EMPLEADO: ver sus propios permisos
        // ============================================================
        [HttpGet]
        public ActionResult MisPermisos()
        {
            var permisos = _repo.ObtenerPermisosPorEmpleado(IdUsuario);
            return View(permisos);
        }

        // ============================================================
        //  EMPLEADO: formulario de solicitud
        // ============================================================
        [HttpGet]
        public ActionResult Solicitar()
        {
            var vm = new SolicitarPermisoVM
            {
                TiposPermiso = _repo.ObtenerTiposPermiso()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Solicitar(SolicitarPermisoVM vm)
        {
            if (vm.FechaFin < vm.FechaInicio)
                ModelState.AddModelError("FechaFin", "La fecha de fin no puede ser anterior al inicio.");

            if (!ModelState.IsValid)
            {
                vm.TiposPermiso = _repo.ObtenerTiposPermiso();
                return View(vm);
            }

            var (ok, mensaje) = _repo.SolicitarPermiso(
                IdUsuario,
                vm.IdTipoPermiso,
                vm.FechaInicio,
                vm.FechaFin,
                vm.Motivo);

            if (ok)
            {
                TempData["Success"] = mensaje;
                return RedirectToAction("MisPermisos");
            }

            ModelState.AddModelError(string.Empty, mensaje);
            vm.TiposPermiso = _repo.ObtenerTiposPermiso();
            return View(vm);
        }

        // ============================================================
        //  ADMIN: ver todos los permisos
        // ============================================================
        [HttpGet]
        public ActionResult Administrar(string estado = null)
        {
            if (!EsAdmin) return new HttpUnauthorizedResult();

            var vm = new AdminPermisosVM
            {
                FiltroEstado = estado,
                Permisos = _repo.ObtenerTodosPermisos(estado),
                Resolver = new ResolverPermisoVM()
            };
            return View(vm);
        }

        // ============================================================
        //  ADMIN: resolver (aprobar / rechazar) desde modal
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resolver(ResolverPermisoVM vm)
        {
            if (!EsAdmin) return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos. Verifique el formulario.";
                return RedirectToAction("Administrar");
            }

            var (ok, mensaje) = _repo.ResolverPermiso(
                vm.IdPermiso,
                IdUsuario,
                vm.NuevoEstado,
                vm.ComentarioAdmin);

            TempData[ok ? "Success" : "Error"] = mensaje;
            return RedirectToAction("Administrar");
        }
    }
}