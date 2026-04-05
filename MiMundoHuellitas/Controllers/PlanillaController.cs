using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiMundoHuellitas.DAL;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.Controllers
{
    public class PlanillaController : Controller
    {
        private readonly PlanillaRepository _repo = new PlanillaRepository();

        [HttpGet]
        public ActionResult Calcular()
        {
            // Pantalla con formulario
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calcular(DateTime fechaInicio, DateTime fechaFin)
        {
<<<<<<< HEAD
            List<PlanillaDetalleVM> resultado = _repo.CalcularPlanilla(fechaInicio, fechaFin, 1.5m, 2.0m, "Cálculo desde MVC");
=======
            string usuarioAuditoria = User.Identity.Name;
            List<PlanillaDetalleVM> resultado = _repo.CalcularPlanilla(fechaInicio, fechaFin, usuarioAuditoria, 1.5m, 2.0m);
>>>>>>> Sebas
            return View("Resultado", resultado);
        }
    }
}