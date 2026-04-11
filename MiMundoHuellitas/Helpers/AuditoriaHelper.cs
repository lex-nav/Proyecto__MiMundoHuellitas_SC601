using MiMundoHuellitas.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiMundoHuellitas.Helpers
{
    public class AuditoriaHelper
    {
        public static void Registrar(
         string tabla,
         string modulo,
         string accion,
         string campo,
         string idRegistro,
         string valorAnterior,
         string valorNuevo,
         string usuario,
         string observacion = null)
                {
            using (var db = new MiMundoHuellitasEntities())
            {
                var log = new MH_Auditoria_TB
                {
                    TablaAfectada = tabla ?? "",
                    Modulo = modulo ?? "",
                    Accion = accion ?? "",
                    Campo = campo ?? "",
                    IdRegistro = idRegistro ?? "",
                    ValorAnterior = valorAnterior ?? "",
                    ValorNuevo = valorNuevo ?? "",
                    UsuarioSistema = usuario ?? "Sistema",
                    FechaAuditoria = DateTime.Now,
                    Observacion = observacion ?? ""
                };

                db.MH_Auditoria_TB.Add(log);
                try
                {
                   db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.InnerException?.InnerException?.Message ?? ex.Message);
                }
            
            }
        }
    }
    }

