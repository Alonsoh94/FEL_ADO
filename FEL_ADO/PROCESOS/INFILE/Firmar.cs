using FEL_ADO.REPOSITORIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Data.SqlClient;

namespace FEL_ADO.PROCESOS.INFILE
{
    public class Firmar
    {
        public string? llave { get; set; }
        public string? archivo { get; set; }
        public string? codigo { get; set; }
        public string? alias { get; set; }
        public string? es_anulacion { get; set; }

        public Firmar()
        {

        }

        #region Request
        dynamic DatosDevueltos;
        public static string? XMLB64Firmado;
        public void FirmarDocumento(Firmar firma, Empresa EmpresaDatosFel, ListaArgumentos Argumentos, string TipoFactura)
        {       

            //  string TipoDocto = tipo.Substring(0, 4);

            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);


            var ObjetoFirmaJSON = JsonConvert.SerializeObject(firma);
            try
            {
                
                string[] RESPUESTAS = new string[3];

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(EmpresaDatosFel.fel_url_firma.Trim());

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "application/json";
              //  ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(ObjetoFirmaJSON);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    DatosDevueltos = JsonConvert.DeserializeObject(result);
                    var RespuestaObtenida = JsonConvert.DeserializeObject(result);

                

                    if (DatosDevueltos.resultado == "false")
                    {
                        if (Argumentos.Tipo_Transaccion == "C")
                        {
                            using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                            {
                                ConexionBitacora.Open();
                                string Estado = "ERROR";
                                string Mensaje = "Error al Firmar: " + DatosDevueltos ;
                                string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                                SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                                cmd.ExecuteNonQuery();
                                ConexionBitacora.Dispose();

                            }
                            using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                            {
                                ActualizarFEL.Open();
                                string QueryDTEs = "update feel_dtes set resultado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                                SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                                cmd.ExecuteNonQuery();
                                ActualizarFEL.Dispose();
                            }
                            Environment.Exit(1);

                        }
                        if (Argumentos.Tipo_Transaccion == "A")
                        {
                            using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                            {
                                ConexionBitacora.Open();
                                string Estado = "ERROR";
                                string Mensaje = "Erro al Firmar: " + DatosDevueltos;
                                string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                                SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                                cmd.ExecuteNonQuery();
                                ConexionBitacora.Dispose();

                            }
                            using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                            {
                                ActualizarFEL.Open();
                                string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                                SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                                cmd.ExecuteNonQuery();
                                ActualizarFEL.Dispose();
                            }
                            Environment.Exit(1);

                        }

                    }
                    if (DatosDevueltos.resultado == "true")
                    {
                        XMLB64Firmado = DatosDevueltos.archivo;
                        //Console.WriteLine(DatosDevueltos);
                    }

                }

            }
            catch (Exception)
            {
                if (DatosDevueltos.resultado == "false")
                {
                    if (Argumentos.Tipo_Transaccion == "C")
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "Error al Firmar: " + DatosDevueltos;
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                            SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                            cmd.ExecuteNonQuery();
                            ConexionBitacora.Dispose();

                        }
                        using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                        {
                            ActualizarFEL.Open();
                            string QueryDTEs = "update feel_dtes set resultado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                            SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                            cmd.ExecuteNonQuery();
                            ActualizarFEL.Dispose();
                        }
                        Environment.Exit(1);

                    }
                    if (Argumentos.Tipo_Transaccion == "A")
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "Error al Firmar: " + DatosDevueltos;
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                            SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                            cmd.ExecuteNonQuery();
                            ConexionBitacora.Dispose();

                        }
                        using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                        {
                            ActualizarFEL.Open();
                            string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                            SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                            cmd.ExecuteNonQuery();
                            ActualizarFEL.Dispose();
                        }
                        Environment.Exit(1);

                    }
                }
            }
        }

        public string ObtenerXMLBase64Firmado() => XMLB64Firmado;
        #endregion

    }

}

