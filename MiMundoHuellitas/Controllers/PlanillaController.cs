using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MiMundoHuellitas.DAL;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.Controllers
{
    [Authorize]
    public class PlanillaController : Controller
    {
        private readonly PlanillaRepository _repo = new PlanillaRepository();

        [HttpGet]
        public ActionResult Calcular()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calcular(DateTime fechaInicio, DateTime fechaFin)
        {
            string usuarioAuditoria = User.Identity.Name;
            List<PlanillaDetalleVM> resultado = _repo.CalcularPlanilla(fechaInicio, fechaFin, usuarioAuditoria, 1.5m, 2.0m);
            return View("Resultado", resultado);
        }

        [HttpGet]
        public ActionResult MisPlanillas()
        {
            int idUsuario = 0;

            if (Session["IdUsuario"] == null || !int.TryParse(Session["IdUsuario"].ToString(), out idUsuario))
            {
                TempData["Error"] = "No se pudo identificar el usuario actual.";
                return RedirectToAction("Login", "Account");
            }

            var model = _repo.ObtenerPlanillasPorUsuario(idUsuario);
            return View(model);
        }

        [HttpGet]
        public ActionResult Detalle(long idPlanilla)
        {
            int idUsuario = 0;

            if (Session["IdUsuario"] == null || !int.TryParse(Session["IdUsuario"].ToString(), out idUsuario))
            {
                TempData["Error"] = "No se pudo identificar el usuario actual.";
                return RedirectToAction("Login", "Account");
            }

            var model = _repo.ObtenerDetallePlanilla(idPlanilla, idUsuario);

            if (model == null)
            {
                TempData["Error"] = "No se encontró la planilla solicitada.";
                return RedirectToAction("MisPlanillas");
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _repo.Dispose();

            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult AdminPlanillas()
        {
            int idTipoUsuario = 0;

            if (Session["IdTipoUsuario"] == null || !int.TryParse(Session["IdTipoUsuario"].ToString(), out idTipoUsuario))
            {
                TempData["Error"] = "No se pudo validar el rol del usuario.";
                return RedirectToAction("Login", "Account");
            }

            // Solo Admin
            if (idTipoUsuario != 2)
            {
                TempData["Error"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            var model = _repo.ObtenerTodasLasPlanillasEmpleados();
            return View(model);
        }

        [HttpGet]
        public ActionResult DetalleAdmin(long idPlanilla, int idUsuario)
        {
            int idTipoUsuario = 0;

            if (Session["IdTipoUsuario"] == null || !int.TryParse(Session["IdTipoUsuario"].ToString(), out idTipoUsuario))
            {
                TempData["Error"] = "No se pudo validar el rol del usuario.";
                return RedirectToAction("Login", "Account");
            }

            if (idTipoUsuario != 2)
            {
                TempData["Error"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            var model = _repo.ObtenerDetallePlanilla(idPlanilla, idUsuario);

            if (model == null)
            {
                TempData["Error"] = "No se encontró la planilla solicitada.";
                return RedirectToAction("AdminPlanillas");
            }

            return View(model);
        }
    }
}