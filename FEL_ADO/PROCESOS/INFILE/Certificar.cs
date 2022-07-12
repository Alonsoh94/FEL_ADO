using FEL_ADO.DTE_SAT.MODULOS;
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
    public class Certificar
    {
        public string? nit_emisor { get; set; }
        public string? correo_copia { get; set; }
        public string? xml_dte { get; set; }
        public Certificar()
        {

        }

        #region Certificar
        dynamic DatosRecibidosCertificacion;
        public void CertificarDocumento(Certificar ocertificar, Empresa EmpresaDatosFel, ListaArgumentos Argumentos, string Referencia, string TipoFactura)
        {
            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);



            try
            {
                var Respuestas = new string[6];
                var ObjetoFirmadoJSON = JsonConvert.SerializeObject(ocertificar);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(EmpresaDatosFel.fel_url_certifica.Trim());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["usuario"] = EmpresaDatosFel.fel_usuario.Trim();
                httpWebRequest.Headers["identificador"] = Referencia;
                httpWebRequest.Headers["llave"] = EmpresaDatosFel.fel_key.Trim();
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
                    DatosRecibidosCertificacion = JsonConvert.DeserializeObject(result);

                    if (DatosRecibidosCertificacion.resultado == "True")
                    {
                        string XMLCertificado = JsonConvert.SerializeObject(DatosRecibidosCertificacion.xml_certificado);

                        string XMLCertificadoReplace = XMLCertificado.Replace("\"", "");
                        byte[] ByteArr = Convert.FromBase64String(XMLCertificadoReplace);
                        string XMLCertificado64convertedToString = ASCIIEncoding.ASCII.GetString(ByteArr);


                        using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                        {
                            ActualizarFEL.Open();
                            string QueryDTEs = "update feel_dtes set resultado = 'CERTIFICADO', xml_resultado = ' " + XMLCertificado64convertedToString + "', uuid = '" + DatosRecibidosCertificacion.uuid + "', numero = '" + DatosRecibidosCertificacion.numero + "', serie = '"+ DatosRecibidosCertificacion.serie+"' WHERE id = " + Argumentos.Id_Documento + ";";

                            SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                            cmd.ExecuteNonQuery();
                            ActualizarFEL.Dispose();
                        }
                        Environment.Exit(0);
                        
                    }
                    else
                    {  
                        string DataRecibida = Convert.ToString(DatosRecibidosCertificacion);
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "Error al Certificar: " + DataRecibida;
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
                }

            }
            catch (Exception)
            {
                string DataRecibida = Convert.ToString(DatosRecibidosCertificacion);
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Error al Certificar: " + DataRecibida ;
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


        }

        #endregion
    }
}
