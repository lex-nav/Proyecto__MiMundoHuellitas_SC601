using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.DAL
{
    public class PlanillaRepository
    {
        private readonly string _cn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

<<<<<<< HEAD
        public List<PlanillaDetalleVM> CalcularPlanilla(DateTime fechaInicio, DateTime fechaFin, decimal factorExtra = 1.5m, decimal factorDoble = 2.0m, string observacion = null)
=======
        public List<PlanillaDetalleVM> CalcularPlanilla(
     DateTime fechaInicio,
     DateTime fechaFin,
     string usuarioAuditoria,
     decimal factorExtra = 1.5m,
     decimal factorDoble = 2.0m,
     string observacion = null)
>>>>>>> Sebas
        {
            var lista = new List<PlanillaDetalleVM>();

            using (var conn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.usp_MH_CalcularPlanilla", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                cmd.Parameters.AddWithValue("@FactorExtra", factorExtra);
                cmd.Parameters.AddWithValue("@FactorDoble", factorDoble);
                cmd.Parameters.AddWithValue("@Observacion", (object)observacion ?? DBNull.Value);

                conn.Open();
<<<<<<< HEAD
=======

                using (var cmdSession = new SqlCommand("EXEC sp_set_session_context @key=N'UsuarioAuditoria', @value=@valor;", conn))
                {
                    cmdSession.Parameters.AddWithValue("@valor", (object)usuarioAuditoria ?? DBNull.Value);
                    cmdSession.ExecuteNonQuery();
                }

>>>>>>> Sebas
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new PlanillaDetalleVM
                        {
                            IdPlanilla = rd.GetInt64(rd.GetOrdinal("IdPlanilla")),
                            FechaInicio = rd.GetDateTime(rd.GetOrdinal("FechaInicio")),
                            FechaFin = rd.GetDateTime(rd.GetOrdinal("FechaFin")),
                            FechaCalculo = rd.GetDateTime(rd.GetOrdinal("FechaCalculo")),

                            IdUsuario = rd.GetInt32(rd.GetOrdinal("IdUsuario")),
                            NombreCompleto = rd.GetString(rd.GetOrdinal("NombreCompleto")),

                            HorasNormales = rd.GetDecimal(rd.GetOrdinal("HorasNormales")),
                            HorasExtra = rd.GetDecimal(rd.GetOrdinal("HorasExtra")),
                            HorasDoble = rd.GetDecimal(rd.GetOrdinal("HorasDoble")),

                            SalarioHora = rd.GetDecimal(rd.GetOrdinal("SalarioHora")),
                            MontoNormales = rd.GetDecimal(rd.GetOrdinal("MontoNormales")),
                            MontoExtra = rd.GetDecimal(rd.GetOrdinal("MontoExtra")),
                            MontoDoble = rd.GetDecimal(rd.GetOrdinal("MontoDoble")),
                            TotalPagar = rd.GetDecimal(rd.GetOrdinal("TotalPagar"))
                        });
                    }
                }
            }

            return lista;
        }
    }
}