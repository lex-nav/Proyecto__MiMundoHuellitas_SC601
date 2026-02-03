using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TuProyecto.DAL
{
    public class MarcacionRepository
    {
        private readonly string _cn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public void MarcarEntrada(int idUsuario)
        {
            EjecutarSP("dbo.usp_MH_MarcarEntrada", idUsuario);
        }

        public void MarcarSalida(int idUsuario)
        {
            EjecutarSP("dbo.usp_MH_MarcarSalida", idUsuario);
        }

        private void EjecutarSP(string spName, int idUsuario)
        {
            using (var conn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
