using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiMundoHuellitas.EF;

namespace MiMundoHuellitas.Controllers 
{ 
    public class SugerenciasController : Controller { 
        
        private MiMundoHuellitasEntities db = new MiMundoHuellitasEntities(); 
        public ActionResult Sugerencias() 

        { var usuario = ObtenerUsuarioActual(); 
            if (usuario == null) return RedirectToAction("Login", "Account"); 
            var citas = db.MH_Cita_TB.Where(c => c.IdUsuario == usuario.IdUsuario).ToList(); 
            var serviciosIds = citas.SelectMany(c => c.MH_Servicios_Cita_TB).Select(s => s.IdServicio).Distinct().ToList(); 
            var productos = db.MH_Servicio_Producto_TB.Where(sp => serviciosIds.Contains(sp.IdServicio)).Select(sp => sp.MH_Productos_TB).Distinct().ToList(); return View(productos); 
        }
        
        private MH_Usuario_TB ObtenerUsuarioActual() 
        { 
            var correo = User.Identity.Name; return db.MH_Usuario_TB.FirstOrDefault(u => u.Correo == correo); 
        } 
    } 
}