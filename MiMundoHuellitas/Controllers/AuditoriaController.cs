using System;
using System.Web.Mvc;
using MiMundoHuellitas.DAL;
using MiMundoHuellitas.Models.ViewModels;
using MiMundoHuellitas.Helpers;

namespace MiMundoHuellitas.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly AuditoriaRepository _repo = new AuditoriaRepository();

        [HttpGet]
        public ActionResult Index(string usuario, string modulo, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var vm = new AuditoriaIndexVM
            {
                Filtro = new AuditoriaFiltroVM
                {
                    Usuario = usuario,
                    Modulo = modulo,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta
                },
                Registros = _repo.ObtenerAuditoria(usuario, modulo, fechaDesde, fechaHasta, 300)
            };

            return View(vm);
        }
    }
}