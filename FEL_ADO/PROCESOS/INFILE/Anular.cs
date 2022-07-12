using FEL_ADO.REPOSITORIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FEL_ADO.PROCESOS.INFILE
{
    public class Anular
    {
        public string? nit_emisor { get; set; }
        public string? correo_copia { get; set; }
        public string? xml_dte { get; set; }

        public Anular()
        {


        }
        dynamic DatosRecibidosAnulacion;
        public void AnularDocumento(Anular OAnular, Empresa DatosFel, ListaArgumentos Argumentos, string Referencia)
        {
            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);


            try
            {
                var DireccionActual = Directory.GetCurrentDirectory();
                var DireccionSave = DireccionActual + "\\ArchivoAnulacion.xml";
                //var ObjetoFirmaJSON = JsonConvert.SerializeObject(certificar); 

                var Respuestas = new string[6];
                var ObjetoFirmadoJSON = JsonConvert.SerializeObject(OAnular);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(DatosFel.fel_url_anula.Trim());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["usuario"] = DatosFel.fel_usuario.Trim();
                httpWebRequest.Headers["identificador"] = Referencia.Trim();
                httpWebRequest.Headers["llave"] = DatosFel.fel_key.Trim();
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(ObjetoFirmadoJSON);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                httpWebRequest.Proxy = null;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    DatosRecibidosAnulacion = JsonConvert.DeserializeObject(result);

                    if (DatosRecibidosAnulacion.resultado == "True")
                    {
                        string XMLCertificado = JsonConvert.SerializeObject(DatosRecibidosAnulacion.xml_certificado);

                        string XMLCertificadoReplace = XMLCertificado.Replace("\"", "");
                        byte[] ByteArr = Convert.FromBase64String(XMLCertificadoReplace);
                        string XMLCertificado64convertedToString = ASCIIEncoding.ASCII.GetString(ByteArr);

                        using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                        {
                            ActualizarFEL.Open();
                            string QueryDTEs = "update feel_anulaciones set resultado = 'CERTIFICADO',estado = 'CERTIFICADO', xml_resultado = ' " + XMLCertificado64convertedToString + "', uuid = '" + DatosRecibidosAnulacion.uuid + "', numero = '" + DatosRecibidosAnulacion.numero + "', serie = '" + DatosRecibidosAnulacion.serie + "' WHERE id = " + Argumentos.Id_Documento + ";";

                            SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                            cmd.ExecuteNonQuery();
                            ActualizarFEL.Dispose();
                        }
                        Environment.Exit(0);
                    }
                    else
                    {
                        string DataRecibida = Convert.ToString(DatosRecibidosAnulacion);
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "Error al Anular: " + DataRecibida;
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "Anulacion" + "','" + Estado + "','" + Mensaje + "');";

                            SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                            cmd.ExecuteNonQuery();
                            ConexionBitacora.Dispose();

                        }
                        using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                        {
                            ActualizarFEL.Open();
                            string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR', estado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                            SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                            cmd.ExecuteNonQuery();
                            ActualizarFEL.Dispose();
                        }
                        Environment.Exit(1);
                    }
                }

            }
            catch (Exception)
            {

                string DataRecibida = Convert.ToString(DatosRecibidosAnulacion);
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Error al Certificar: " + DataRecibida;
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "Anulacion" + "','" + Estado + "','" + Mensaje + "');";

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
