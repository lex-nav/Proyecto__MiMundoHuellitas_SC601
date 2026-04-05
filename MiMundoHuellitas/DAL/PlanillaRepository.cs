using MiMundoHuellitas.EF;
using MiMundoHuellitas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MiMundoHuellitas.DAL
{
    public class PlanillaRepository : IDisposable
    {
        private readonly BD_MiMundoHuellitasEntities _db;

        public PlanillaRepository()
        {
            _db = new BD_MiMundoHuellitasEntities();
        }

        public List<PlanillaDetalleVM> CalcularPlanilla(DateTime fechaInicio, DateTime fechaFin, string usuarioAuditoria, decimal factorExtra, decimal factorDoble)
        {
            var pFechaInicio = new SqlParameter("@FechaInicio", fechaInicio.Date);
            var pFechaFin = new SqlParameter("@FechaFin", fechaFin.Date);
            var pFactorExtra = new SqlParameter("@FactorExtra", factorExtra);
            var pFactorDoble = new SqlParameter("@FactorDoble", factorDoble);
            var pObservacion = new SqlParameter("@Observacion", $"Cálculo generado por {usuarioAuditoria}");

            var result = _db.Database.SqlQuery<PlanillaDetalleVM>(
                @"EXEC dbo.usp_MH_CalcularPlanilla 
                    @FechaInicio,
                    @FechaFin,
                    @FactorExtra,
                    @FactorDoble,
                    @Observacion",
                pFechaInicio, pFechaFin, pFactorExtra, pFactorDoble, pObservacion
            ).ToList();

            return result;
        }

        public List<PlanillaDetalleVM> ObtenerPlanillasPorUsuario(int idUsuario)
        {
            string sql = @"
                SELECT 
                    IdPlanilla,
                    FechaInicio,
                    FechaFin,
                    FechaCalculo,
                    IdUsuario,
                    NombreCompleto,
                    HorasNormales,
                    HorasExtra,
                    HorasDoble,
                    SalarioHora,
                    MontoNormales,
                    MontoExtra,
                    MontoDoble,
                    TotalPagar
                FROM dbo.vw_MH_PlanillaDetalle
                WHERE IdUsuario = @IdUsuario
                ORDER BY FechaInicio DESC, IdPlanilla DESC";

            var pIdUsuario = new SqlParameter("@IdUsuario", idUsuario);

            return _db.Database.SqlQuery<PlanillaDetalleVM>(sql, pIdUsuario).ToList();
        }

        public PlanillaDetalleVM ObtenerDetallePlanilla(long idPlanilla, int idUsuario)
        {
            string sql = @"
        SELECT TOP 1
            IdPlanilla,
            FechaInicio,
            FechaFin,
            FechaCalculo,
            IdUsuario,
            NombreCompleto,
            HorasNormales,
            HorasExtra,
            HorasDoble,
            SalarioHora,
            MontoNormales,
            MontoExtra,
            MontoDoble,
            TotalPagar
        FROM dbo.vw_MH_PlanillaDetalle
        WHERE IdPlanilla = @IdPlanilla
          AND IdUsuario = @IdUsuario";

            var pIdPlanilla = new System.Data.SqlClient.SqlParameter("@IdPlanilla", idPlanilla);
            var pIdUsuario = new System.Data.SqlClient.SqlParameter("@IdUsuario", idUsuario);

            return _db.Database.SqlQuery<PlanillaDetalleVM>(sql, pIdPlanilla, pIdUsuario).FirstOrDefault();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public List<PlanillaDetalleVM> ObtenerTodasLasPlanillasEmpleados()
        {
            string sql = @"
        SELECT
            IdPlanilla,
            FechaInicio,
            FechaFin,
            FechaCalculo,
            IdUsuario,
            NombreCompleto,
            HorasNormales,
            HorasExtra,
            HorasDoble,
            SalarioHora,
            MontoNormales,
            MontoExtra,
            MontoDoble,
            TotalPagar
        FROM dbo.vw_MH_PlanillaDetalle
        ORDER BY FechaInicio DESC, NombreCompleto ASC, IdPlanilla DESC";

            return _db.Database.SqlQuery<PlanillaDetalleVM>(sql).ToList();
        }
    }
}