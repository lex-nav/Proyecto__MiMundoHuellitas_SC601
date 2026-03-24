using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.DAL
{
    public class AuditoriaRepository
    {
        private readonly string _cn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public List<AuditoriaVM> ObtenerAuditoria(
            string usuario = null,
            string modulo = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int top = 200)
        {
            var lista = new List<AuditoriaVM>();

            using (var conn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand(@"
                SELECT TOP (@top)
                    IdAuditoria,
                    TablaAfectada,
                    Modulo,
                    Accion,
                    Campo,
                    IdRegistro,
                    ValorAnterior,
                    ValorNuevo,
                    UsuarioSistema,
                    FechaAuditoria,
                    Observacion
                FROM dbo.MH_Auditoria_TB
                WHERE
                    (@Usuario IS NULL OR UsuarioSistema LIKE '%' + @Usuario + '%')
                    AND (@Modulo IS NULL OR Modulo = @Modulo)
                    AND (@FechaDesde IS NULL OR CAST(FechaAuditoria AS DATE) >= @FechaDesde)
                    AND (@FechaHasta IS NULL OR CAST(FechaAuditoria AS DATE) <= @FechaHasta)
                ORDER BY FechaAuditoria DESC, IdAuditoria DESC", conn))
            {
                cmd.Parameters.AddWithValue("@top", top);
                cmd.Parameters.AddWithValue("@Usuario", (object)usuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Modulo", (object)modulo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaDesde", (object)fechaDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaHasta", (object)fechaHasta ?? DBNull.Value);

                conn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new AuditoriaVM
                        {
                            IdAuditoria = rd.GetInt32(rd.GetOrdinal("IdAuditoria")),
                            TablaAfectada = rd.GetString(rd.GetOrdinal("TablaAfectada")),
                            Modulo = rd.GetString(rd.GetOrdinal("Modulo")),
                            Accion = rd.GetString(rd.GetOrdinal("Accion")),
                            Campo = rd.GetString(rd.GetOrdinal("Campo")),
                            IdRegistro = rd.GetString(rd.GetOrdinal("IdRegistro")),
                            ValorAnterior = rd["ValorAnterior"]?.ToString(),
                            ValorNuevo = rd["ValorNuevo"]?.ToString(),
                            UsuarioSistema = rd.GetString(rd.GetOrdinal("UsuarioSistema")),
                            FechaAuditoria = rd.GetDateTime(rd.GetOrdinal("FechaAuditoria")),
                            Observacion = rd["Observacion"]?.ToString()
                        });
                    }
                }
            }

            return lista;
        }
    }
}