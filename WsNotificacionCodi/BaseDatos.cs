using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
//using System.Runtime.Serialization.

namespace WsNotificacionCodi
{
    public static class BaseDatos
    {
        public static string RutaLog = null;
        //const string SERVIDOR = "192.168.123.44";
        const string SERVIDOR = "192.168.162.3";
        const string USUARIO = "apiCodi";
        const string CONTRASENIA = "YaRmENriAL";
        const string NOMBRE_BASE = "SOFTCREDITO";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static string cadenaConexion = "Data Source=" + SERVIDOR + ";Initial Catalog=" + NOMBRE_BASE + ";User id=" + USUARIO + ";Password=" + CONTRASENIA;

        /*
            Referencia //campo "des" de peticion de QR? o campo "ref" de 7 caracteres
            Banco
            FechaHora
            Descripcion
            Usuario
            Contrasenia
            Monto
            TipoPago
            Folio
            Sucursal
        */

        private static void init() {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(RutaLog));
        }
        private static string getSHA1(string texto)
        {
            SHA1 sha1 = SHA1Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha1.ComputeHash(encoding.GetBytes(texto));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        public static bool Antenticar(string usuario,string contrasenia)
        {
            init();
            log.Info("Método -Antenticar-");
            bool valido = false;
            string contraseniaSha1 = getSHA1(contrasenia);

            SqlConnection conexion = new SqlConnection();
            try
            {
                conexion.ConnectionString = cadenaConexion;
                conexion.Open();

                string consultaSql = "sp_mpf_validar_autenticacion_codi";
                SqlCommand comando = new SqlCommand(consultaSql, conexion);
                SqlParameter usuarioParam = new SqlParameter("@usuario", usuario);
                SqlParameter contraseniaParam = new SqlParameter("@contrasenia", contraseniaSha1);
                comando.Parameters.Add(usuarioParam);
                comando.Parameters.Add(contraseniaParam);
                comando.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader reader = comando.ExecuteReader();

                if (reader.HasRows)
                {
                    valido = true;
                }

                reader.Close();
                comando.Dispose();
                conexion.Close();
                return valido;
            }
            catch (Exception ex)
            {
                log.Error("Ocurrió un error -Antenticar-: " + ex.Message);
                valido = false;
                return valido;
            }
            finally
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                    conexion.Dispose();
                }
            }
        }
        public static string RegistrarPago(ObjPeticion peticion)
        {
            init();
            log.Info("Método -RegistrarPago-");
            //Serializando...
            var serializer = new JavaScriptSerializer();
            var serializedResult = serializer.Serialize(peticion);           
            log.Info("objPeticion: " + serializedResult);

            SqlConnection conexion = new SqlConnection();
            string idRegistrado = "";

            try
            {
                conexion.ConnectionString = cadenaConexion;
                conexion.Open();

                string consultaSql = "sp_mpf_inserta_pago_codi"; //falta que este procedimiento, inserte en la tabla macropay o llamar el procedimiento que lo hace dentro de éste.
                SqlCommand comando = new SqlCommand();
                comando.Connection = conexion;
                comando.CommandText = consultaSql;
                comando.CommandType = System.Data.CommandType.StoredProcedure;

                SqlParameter tipoParam = new SqlParameter("@tipo", 1);
                SqlParameter referenciaParam = new SqlParameter("@referencia", peticion.Referencia);
                SqlParameter bancoParam = new SqlParameter("@banco", peticion.Banco);
                SqlParameter fechaHoraParam = new SqlParameter("@fecha_hora", peticion.FechaHora + ".000"); //Formato que debe ser enviado al servicio : "18-03-2021 13:45:00"
                SqlParameter descripcionParam = new SqlParameter("@descripcion", peticion.Descripcion);
                SqlParameter usuarioParam = new SqlParameter("@usuario", peticion.Usuario);
                SqlParameter montoParam = new SqlParameter("@monto", peticion.Monto);
                SqlParameter tipoPagoParam = new SqlParameter("@tipo_pago", peticion.TipoPago);
                SqlParameter folioParam = new SqlParameter("@folio", peticion.Folio);
                SqlParameter sucursalParam = new SqlParameter("@sucursal", peticion.Sucursal);
                SqlParameter usuarioAltaParam = new SqlParameter("@usuario_alta", "App");
                var outParm = new SqlParameter("@id_devuelto", SqlDbType.VarChar);
                outParm.Direction = ParameterDirection.Output;
                outParm.Size = 30;

                comando.Parameters.Add(tipoParam);
                comando.Parameters.Add(referenciaParam);
                comando.Parameters.Add(bancoParam);
                comando.Parameters.Add(fechaHoraParam);
                comando.Parameters.Add(descripcionParam);
                comando.Parameters.Add(usuarioParam);
                comando.Parameters.Add(montoParam);
                comando.Parameters.Add(tipoPagoParam);
                comando.Parameters.Add(folioParam);
                comando.Parameters.Add(sucursalParam);
                comando.Parameters.Add(usuarioAltaParam);
                comando.Parameters.Add(outParm);
                comando.ExecuteNonQuery();
                comando.Dispose();
                conexion.Close();

                if (outParm.Value.ToString() != "")
                {
                    idRegistrado = outParm.Value.ToString();
                }

                return idRegistrado;
            }
            catch (Exception ex)
            {
                log.Error("Ocurrió un error -RegistrarPago- : " + ex.Message);
                return idRegistrado;
            }
            finally
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                    conexion.Dispose();
                }
            }

        }

        public static bool GenerarAbono(ObjPeticion peticion, string folio)
        {
            init();
            log.Info("Método -GenerarAbono-");
            SqlConnection conexion = new SqlConnection();
            

            try
            {
                conexion.ConnectionString = cadenaConexion;
                conexion.Open();

                string consultaSql = "sp_sfc_generar_abono"; 
                SqlCommand comando = new SqlCommand();
                comando.Connection = conexion;
                comando.CommandText = consultaSql;
                comando.CommandType = System.Data.CommandType.StoredProcedure;
                
                SqlParameter claveSolicitud =   new SqlParameter("@clave_solicitud", peticion.Referencia);
                SqlParameter cveSucursal =      new SqlParameter("@cve_sucursal", "CODI");
                SqlParameter cveVendedor =      new SqlParameter("@cve_vendedor", folio);                 
                SqlParameter caja =             new SqlParameter("@caja", "000005");
                SqlParameter totalPagado =      new SqlParameter("@total_pagado", peticion.Monto);                
                SqlParameter cveFormaPago =     new SqlParameter("@cve_forma_pago", "0001");
                                
                comando.Parameters.Add(claveSolicitud);
                comando.Parameters.Add(cveSucursal);
                comando.Parameters.Add(cveVendedor);
                comando.Parameters.Add(caja);
                comando.Parameters.Add(totalPagado);
                comando.Parameters.Add(cveFormaPago);
                
                comando.ExecuteNonQuery();
                comando.Dispose();
                conexion.Close();
               
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Ocurrió un error -GenerarAbono- : " + ex.Message);
                return false;
            }
            finally
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                    conexion.Dispose();
                }
            }

        }

        /*Ejemplo soap body
         <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:not="http://notificacion.macropay.mx/">
           <soapenv:Header/>
           <soapenv:Body>
              <not:Notify>
                 <not:objPeticion>
                    <not:Referencia>454654655665</not:Referencia>
                    <not:Banco>04</not:Banco>
                    <not:FechaHora>18-03-2021 13:45:20</not:FechaHora>
                    <not:Descripcion>Ejemplo prueba</not:Descripcion>
                    <not:Usuario>8$32Ra58</not:Usuario>
                    <not:Contrasenia>A7859gHy</not:Contrasenia>
                    <not:Monto>152</not:Monto>
                    <not:TipoPago>15</not:TipoPago>
                    <not:Folio>10</not:Folio>
                    <not:Sucursal>12</not:Sucursal>
                 </not:objPeticion>
              </not:Notify>
           </soapenv:Body>
        </soapenv:Envelope>
        */

    }
}