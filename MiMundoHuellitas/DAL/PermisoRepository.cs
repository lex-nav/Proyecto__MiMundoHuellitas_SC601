using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.DAL
{
    public class PermisoRepository
    {
        private readonly string _conn =
           ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // ----------------------------------------------------------------
        // CATÁLOGO DE TIPOS DE PERMISO
        // ----------------------------------------------------------------
        public List<TipoPermisoVM> ObtenerTiposPermiso()
        {
            const string sql = @"
                SELECT IdTipoPermiso, Nombre, Descripcion, RequiereDoc
                FROM   dbo.MH_TipoPermiso_TB
                WHERE  Activo = 1
                ORDER  BY Nombre";

            var lista = new List<TipoPermisoVM>();

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new TipoPermisoVM
                        {
                            IdTipoPermiso = (int)dr["IdTipoPermiso"],
                            Nombre = dr["Nombre"].ToString(),
                            Descripcion = dr["Descripcion"] == DBNull.Value ? null : dr["Descripcion"].ToString(),
                            RequiereDoc = (bool)dr["RequiereDoc"]
                        });
                    }
                }
            }
            return lista;
        }

        // ----------------------------------------------------------------
        // SOLICITAR PERMISO (empleado)
        // Valida solapamiento en código antes de insertar
        // ----------------------------------------------------------------
        public (bool ok, string mensaje) SolicitarPermiso(
            int idUsuario, int idTipoPermiso,
            DateTime fechaInicio, DateTime fechaFin, string motivo)
        {
            if (fechaFin.Date < fechaInicio.Date)
                return (false, "La fecha de fin no puede ser anterior al inicio.");

            // --- Verificar solapamiento ---
            const string sqlSolape = @"
                SELECT COUNT(1)
                FROM   dbo.MH_Permiso_TB
                WHERE  IdUsuario    = @IdUsuario
                  AND  Estado       IN ('Pendiente','Aprobado')
                  AND  FechaInicio <= @FechaFin
                  AND  FechaFin    >= @FechaInicio";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                using (var cmdCheck = new SqlCommand(sqlSolape, con))
                {
                    cmdCheck.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmdCheck.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmdCheck.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                    int solapados = (int)cmdCheck.ExecuteScalar();
                    if (solapados > 0)
                        return (false, "Ya existe un permiso aprobado o pendiente en ese rango de fechas.");
                }

                // --- Insertar ---
                const string sqlInsert = @"
                    INSERT INTO dbo.MH_Permiso_TB
                        (IdUsuario, IdTipoPermiso, FechaInicio, FechaFin, Motivo, Estado, FechaSolicitud)
                    VALUES
                        (@IdUsuario, @IdTipoPermiso, @FechaInicio, @FechaFin, @Motivo, 'Pendiente', GETDATE())";

                using (var cmdIns = new SqlCommand(sqlInsert, con))
                {
                    cmdIns.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmdIns.Parameters.AddWithValue("@IdTipoPermiso", idTipoPermiso);
                    cmdIns.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmdIns.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                    cmdIns.Parameters.AddWithValue("@Motivo", motivo);

                    cmdIns.ExecuteNonQuery();
                }
            }

            return (true, "Solicitud enviada correctamente.");
        }

        // ----------------------------------------------------------------
        // PERMISOS DE UN EMPLEADO (solo los suyos)
        // ----------------------------------------------------------------
        public List<PermisoVM> ObtenerPermisosPorEmpleado(int idUsuario)
        {
            const string sql = @"
                SELECT
                    p.IdPermiso,
                    p.IdUsuario,
                    NULL                                        AS Empleado,
                    tp.Nombre                                   AS TipoPermiso,
                    p.FechaInicio,
                    p.FechaFin,
                    DATEDIFF(DAY, p.FechaInicio, p.FechaFin)+1 AS DiasSolicitados,
                    p.Motivo,
                    p.Estado,
                    p.FechaSolicitud,
                    p.ComentarioAdmin,
                    p.FechaResolucion,
                    adm.NombreCompleto                          AS AdminResolvio
                FROM       dbo.MH_Permiso_TB       p
                INNER JOIN dbo.MH_TipoPermiso_TB   tp  ON tp.IdTipoPermiso  = p.IdTipoPermiso
                LEFT  JOIN dbo.MH_Usuario_TB        adm ON adm.IdUsuario     = p.IdAdminResolucion
                WHERE p.IdUsuario = @IdUsuario
                ORDER BY p.FechaSolicitud DESC";

            return EjecutarListaPermisos(sql,
                cmd => cmd.Parameters.AddWithValue("@IdUsuario", idUsuario),
                incluyeEmpleado: false);
        }

        // ----------------------------------------------------------------
        // TODOS LOS PERMISOS (admin) con filtro opcional de estado
        // ----------------------------------------------------------------
        public List<PermisoVM> ObtenerTodosPermisos(string soloEstado = null)
        {
            // El filtro de estado es opcional; si es null trae todos
            string sql = @"
                SELECT
                    p.IdPermiso,
                    p.IdUsuario,
                    emp.NombreCompleto                          AS Empleado,
                    tp.Nombre                                   AS TipoPermiso,
                    p.FechaInicio,
                    p.FechaFin,
                    DATEDIFF(DAY, p.FechaInicio, p.FechaFin)+1 AS DiasSolicitados,
                    p.Motivo,
                    p.Estado,
                    p.FechaSolicitud,
                    p.ComentarioAdmin,
                    p.FechaResolucion,
                    adm.NombreCompleto                          AS AdminResolvio
                FROM       dbo.MH_Permiso_TB       p
                INNER JOIN dbo.MH_Usuario_TB        emp ON emp.IdUsuario     = p.IdUsuario
                INNER JOIN dbo.MH_TipoPermiso_TB   tp  ON tp.IdTipoPermiso  = p.IdTipoPermiso
                LEFT  JOIN dbo.MH_Usuario_TB        adm ON adm.IdUsuario     = p.IdAdminResolucion
                WHERE (@SoloEstado IS NULL OR p.Estado = @SoloEstado)
                ORDER BY
                    CASE p.Estado WHEN 'Pendiente' THEN 0 ELSE 1 END,
                    p.FechaSolicitud DESC";

            return EjecutarListaPermisos(sql, cmd =>
            {
                cmd.Parameters.Add("@SoloEstado", SqlDbType.NVarChar, 20).Value =
                    string.IsNullOrEmpty(soloEstado) ? (object)DBNull.Value : soloEstado;
            }, incluyeEmpleado: true);
        }

        // ----------------------------------------------------------------
        // RESOLVER PERMISO (admin: aprobar o rechazar)
        // ----------------------------------------------------------------
        public (bool ok, string mensaje) ResolverPermiso(
            long idPermiso, int idAdmin, string nuevoEstado, string comentario)
        {
            if (nuevoEstado != "Aprobado" && nuevoEstado != "Rechazado")
                return (false, "Estado inválido.");

            // Verificar que exista y esté pendiente
            const string sqlCheck = @"
                SELECT COUNT(1)
                FROM   dbo.MH_Permiso_TB
                WHERE  IdPermiso = @IdPermiso
                  AND  Estado    = 'Pendiente'";

            const string sqlUpdate = @"
                UPDATE dbo.MH_Permiso_TB
                SET    Estado            = @NuevoEstado,
                       IdAdminResolucion = @IdAdmin,
                       FechaResolucion   = GETDATE(),
                       ComentarioAdmin   = @Comentario
                WHERE  IdPermiso = @IdPermiso
                  AND  Estado    = 'Pendiente'";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                using (var cmdCheck = new SqlCommand(sqlCheck, con))
                {
                    cmdCheck.Parameters.AddWithValue("@IdPermiso", idPermiso);
                    int existe = (int)cmdCheck.ExecuteScalar();
                    if (existe == 0)
                        return (false, "El permiso no existe o ya fue resuelto.");
                }

                using (var cmdUpd = new SqlCommand(sqlUpdate, con))
                {
                    cmdUpd.Parameters.AddWithValue("@IdPermiso", idPermiso);
                    cmdUpd.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);
                    cmdUpd.Parameters.AddWithValue("@IdAdmin", idAdmin);
                    cmdUpd.Parameters.Add("@Comentario", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(comentario) ? (object)DBNull.Value : comentario;

                    int filas = cmdUpd.ExecuteNonQuery();
                    if (filas == 0)
                        return (false, "No se pudo actualizar el permiso.");
                }
            }

            return (true, $"Permiso {nuevoEstado.ToLower()} correctamente.");
        }

        // ----------------------------------------------------------------
        // Helper privado: ejecutar query y mapear lista de PermisoVM
        // ----------------------------------------------------------------
        private List<PermisoVM> EjecutarListaPermisos(
            string sql,
            Action<SqlCommand> parametros,
            bool incluyeEmpleado)
        {
            var lista = new List<PermisoVM>();

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(sql, con))
            {
                parametros(cmd);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new PermisoVM
                        {
                            IdPermiso = Convert.ToInt64(dr["IdPermiso"]),
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Empleado = incluyeEmpleado ? dr["Empleado"].ToString() : null,
                            TipoPermiso = dr["TipoPermiso"].ToString(),
                            FechaInicio = Convert.ToDateTime(dr["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(dr["FechaFin"]),
                            DiasSolicitados = Convert.ToInt32(dr["DiasSolicitados"]),
                            Motivo = dr["Motivo"].ToString(),
                            Estado = dr["Estado"].ToString(),
                            FechaSolicitud = Convert.ToDateTime(dr["FechaSolicitud"]),
                            ComentarioAdmin = dr["ComentarioAdmin"] == DBNull.Value ? null : dr["ComentarioAdmin"].ToString(),
                            FechaResolucion = dr["FechaResolucion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaResolucion"]),
                            AdminResolvio = dr["AdminResolvio"] == DBNull.Value ? null : dr["AdminResolvio"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}