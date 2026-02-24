using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MiMundoHuellitas.DAL
{
    public class PlanillaCierreRepository
    {
        private readonly string _cn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public (bool Ok, string Mensaje) CerrarPeriodo(DateTime fechaInicio, DateTime fechaFin, int cerradaPorIdUsuario, string motivo)
        {
            using (var conn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.usp_MH_CerrarPlanilla", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                cmd.Parameters.AddWithValue("@CerradaPorIdUsuario", cerradaPorIdUsuario);
                cmd.Parameters.AddWithValue("@Motivo", (object)motivo ?? DBNull.Value);

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        bool ok = rd.GetInt32(rd.GetOrdinal("Ok")) == 1;
                        string msg = rd.GetString(rd.GetOrdinal("Mensaje"));
                        return (ok, msg);
                    }
                }
            }
            return (false, "No se pudo cerrar la planilla.");
        }
    }
}
