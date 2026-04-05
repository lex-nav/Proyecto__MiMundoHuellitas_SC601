using MiMundoHuellitas.EF;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MiMundoHuellitas.DAL
{
    public class MarcacionRepository : IDisposable
    {
        private readonly BD_MiMundoHuellitasEntities _db;

        public MarcacionRepository()
        {
            _db = new BD_MiMundoHuellitasEntities();
        }

        public void MarcarEntrada(int idUsuario)
        {
            var pIdUsuario = new SqlParameter("@IdUsuario", SqlDbType.Int)
            {
                Value = idUsuario
            };

            _db.Database.ExecuteSqlCommand(
                "EXEC dbo.usp_MH_MarcarEntrada @IdUsuario",
                pIdUsuario
            );
        }

        public void MarcarSalida(int idUsuario)
        {
            var pIdUsuario = new SqlParameter("@IdUsuario", SqlDbType.Int)
            {
                Value = idUsuario
            };

            _db.Database.ExecuteSqlCommand(
                "EXEC dbo.usp_MH_MarcarSalida @IdUsuario",
                pIdUsuario
            );
        }

        public void CerrarMarcacionesAnterioresAbiertas(int idUsuario)
        {
            var abiertas = _db.MH_Marcacion_TB
                .Where(x => x.IdUsuario == idUsuario && x.HoraSalida == null && x.Aprobada)
                .ToList();

            foreach (var item in abiertas)
            {
                DateTime fechaBase = item.Fecha;
                DateTime salidaAuto = fechaBase.AddHours(17); // 5 PM por defecto

                if (item.HoraEntrada > salidaAuto)
                    salidaAuto = item.HoraEntrada.AddHours(1);

                item.HoraSalida = salidaAuto;
                item.Observacion = (item.Observacion ?? "") + " | Cerrada automáticamente por nuevo login";
            }

            if (abiertas.Any())
                _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
