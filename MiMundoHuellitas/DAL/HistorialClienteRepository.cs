using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MiMundoHuellitas.Models.ViewModels;

namespace MiMundoHuellitas.DAL
{
    public class HistorialClienteRepository
    {
        private readonly string _conn =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ClienteHistorialVM ObtenerCliente(int idUsuario)
        {
            ClienteHistorialVM cliente = null;

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("usp_MH_ObtenerClientePorId", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        cliente = new ClienteHistorialVM
                        {
                            IdUsuario = (int)dr["IdUsuario"],
                            NombreCompleto = dr["NombreCompleto"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                            TipoUsuario = dr["TipoUsuario"].ToString(),
                            Activo = (bool)dr["Activo"]
                        };
                    }
                }
            }

            return cliente;
        }

        public List<MascotaHistorialVM> ObtenerMascotas(int idUsuario)
        {
            var lista = new List<MascotaHistorialVM>();

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("usp_MH_ObtenerMascotasPorCliente", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new MascotaHistorialVM
                        {
                            IdMascota = (int)dr["IdMascota"],
                            NombreMascota = dr["NombreMascota"].ToString(),
                            Especie = dr["Especie"].ToString(),
                            Raza = dr["Raza"] == DBNull.Value ? null : dr["Raza"].ToString(),
                            FechaNacimiento = dr["FechaNacimiento"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(dr["FechaNacimiento"]),
                            Activo = (bool)dr["Activo"]
                        });
                    }
                }
            }

            return lista;
        }

        public List<CitaHistorialVM> ObtenerCitas(int idUsuario)
        {
            var lista = new List<CitaHistorialVM>();

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("usp_MH_ObtenerCitasPorCliente", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new CitaHistorialVM
                        {
                            IdCita = (int)dr["IdCita"],
                            FechaHoraCita = (DateTime)dr["FechaHoraCita"],
                            Estado = dr["Estado"].ToString(),
                            Mascota = dr["Mascota"].ToString(),
                            Servicio = dr["Servicio"] == DBNull.Value ? null : dr["Servicio"].ToString(),
                            PrecioUnitario = dr["PrecioUnitario"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(dr["PrecioUnitario"]),
                            Subtotal = dr["Subtotal"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(dr["Subtotal"])
                        });
                    }
                }
            }

            return lista;
        }

        public List<FacturaHistorialVM> ObtenerFacturas(int idUsuario)
        {
            var lista = new List<FacturaHistorialVM>();

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("usp_MH_ObtenerFacturasPorCliente", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new FacturaHistorialVM
                        {
                            IdFactura = (int)dr["IdFactura"],
                            FechaFactura = (DateTime)dr["FechaFactura"],
                            Estado = dr["Estado"].ToString(),
                            MetodoPago = dr["MetodoPago"] == DBNull.Value ? null : dr["MetodoPago"].ToString(),
                            MontoTotal = Convert.ToDecimal(dr["MontoTotal"]),
                            NumeroReferencia = dr["NumeroReferencia"] == DBNull.Value ? null : dr["NumeroReferencia"].ToString()
                        });
                    }
                }
            }

            return lista;
        }
    }
}