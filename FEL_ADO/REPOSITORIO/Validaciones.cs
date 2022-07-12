using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using FEL_ADO.DTE_SAT.XML;

namespace FEL_ADO.REPOSITORIO
{
    public class Validaciones
    {
        XMLCertificar oXmlCertificar = new XMLCertificar();

        #region Validacion de Argumentos
        public void ValidacionDatos(Array Args)
        {
            int Paramentros = 9;
            int Contador = 0;
            foreach (dynamic item in Args)
            {
                if (item.ToString().Trim() == string.Empty)
                {
                    Console.WriteLine("No se aceptan valores vacios o espacios en blaco como paramentros");
                    Environment.Exit(1);
                }
                Contador++;
            }
            if (Contador != Paramentros)
            {
                Console.WriteLine("Faltan parametros para proceder con la ejecución del programa, Vefique e intentelo de nuevo. Paramentros requeridos: " + Paramentros + ", Pametros recibidos: " + Contador);
                Environment.Exit(1);
            }
           
        }
        #endregion
        Empresa EmpresaDatosFEl = new Empresa();
        #region Verificar datos Empresa
        public void VerificarDatosEmpresa(ListaArgumentos Argumentos)
        {
            try
            {
                string ConnectionStringEmpresa = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseEmpresa, Argumentos.Usuario, Argumentos.clave);
                
                using (SqlConnection Conexion = new SqlConnection(ConnectionStringEmpresa))
                {
                    string QueryDatosCertificador = "select fel_certificador, fel_usuario, fel_clave, fel_key,fel_token, fel_vencimiento_token, fel_hora_vencimiento_token, fel_url_firma, fel_url_certifica,fel_url_anula, fel_url_Token, Path_XML  from EMPRESA where NUMERO = " + Argumentos.Id_Empresa + ";";

                    
                    Conexion.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryDatosCertificador, Conexion);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosEmpresa = ds.Tables[0];

                    if (TablaDatosEmpresa.Rows.Count != 0)
                    {
                        foreach (DataRow item in TablaDatosEmpresa.Rows)
                        {
                            if (Convert.ToString(item[0]).Trim() == string.Empty & Convert.ToString(item[3]).Trim() == string.Empty & Convert.ToString(item[4]).Trim() == string.Empty & Convert.ToString(item[7]).Trim() == string.Empty & Convert.ToString(item[8]).Trim() == string.Empty & Convert.ToString(item[9]).Trim() == string.Empty)
                            {
                                string StringConexionBitacora = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionBitacora))
                                {
                                    ConexionBitacora.Open();
                                    string Estado = "ERROR";
                                    string Mensaje = "Algunos datos de Certificación son Necesarios, verifique te tenga los datos necesarios para proceder con su solicitud";
                                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                                    cmd.ExecuteNonQuery();
                                    ConexionBitacora.Dispose();
                                    Environment.Exit(1);
                                }


                            }
                            var ObjetoEmpresa = new Empresa()
                            {
                                Fel_certificador = Convert.ToString(item[0]).Trim(),
                                fel_usuario = Convert.ToString(item[1]).Trim(),
                                fel_clave = Convert.ToString(item[2]).Trim(),
                                fel_key = Convert.ToString(item[3]).Trim(),
                                fel_token = Convert.ToString(item[4]).Trim(),
                                fel_vencimiento_token = Convert.ToString(item[5]).Trim(),
                                fel_hora_vencimiento_token = Convert.ToString(item[6]).Trim(),
                                fel_url_firma = Convert.ToString(item[7]).Trim(),
                                fel_url_certifica = Convert.ToString(item[8]).Trim(),
                                fel_url_anula = Convert.ToString(item[9]).Trim(),
                                fel_url_Token = Convert.ToString(item[10]).Trim(),
                                Path_XML = Convert.ToString(item[11]).Trim()

                            };
                            EmpresaDatosFEl = ObjetoEmpresa;
                           
                        }

                    }
                    else
                    {
                        string StringConexionBitacora = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionBitacora))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se ha podido comprobar los datos de Empresa con Numero de Empresa proporcionado.";
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                            SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                            cmd.ExecuteNonQuery();
                            ConexionBitacora.Dispose();
                            Environment.Exit(1);
                        }
                    }

                    Conexion.Dispose();
                }

            }
            catch (Exception e)
            {

                string StringConexionBitacora = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionBitacora))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Algo Salio Mal al intentar comprobar los datos de Empresa, favor verificar los datos y volver a intentarlo : ";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();
                    Environment.Exit(1);
                }
            }
           

        }

        #endregion
        #region Existe el Documento de Certificacione
        public void ExisteElDocumentoCertificacion(ListaArgumentos Argumentos)
        {
            try
            {
                string StringConexionFEL= string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionFEL = new SqlConnection(StringConexionFEL))
                {
                    string QueryDatosCertificador = "select resultado,dg_tipo, dg_exportacion from feel_dtes where id =" + Argumentos.Id_Documento + ";";

                    ConexionFEL.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryDatosCertificador, ConexionFEL);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosEmpresa = ds.Tables[0];
                    if(TablaDatosEmpresa.Rows.Count  != 0)
                    {
                        string? Tipo = string.Empty;
                        string? Resultado = string.Empty;
                        string? EXPO = string.Empty;
                        foreach (DataRow item in TablaDatosEmpresa.Rows)
                        {
                            Resultado = Convert.ToString(item[0]).Trim();
                            Tipo = Convert.ToString(item[1]).Trim();
                            EXPO = Convert.ToString(item[2]).Trim();

                        }
                        if (Resultado == "PENDIENTE")
                        {
                            using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                            {
                                ActualizarFEL.Open();
                                string QueryDTEs = "update feel_dtes set resultado = 'EN PROCESO' WHERE id = " + Argumentos.Id_Documento + ";";

                                SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                                cmd.ExecuteNonQuery();
                                ActualizarFEL.Dispose();
                            }
                            oXmlCertificar.EjecutorCertificaciones(Argumentos,EmpresaDatosFEl, EXPO, Tipo);
                        }
                        else
                        {

                            using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                            {
                                
                                ConexionBitacora.Open();                               
                                string Estado = "ERROR";
                                string Mensaje = "El Id Proporcionado tiene un resultado diferente a PENDIENTE y no podemos procesar su solicitud en un resultado diferente. ";
                                string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + Tipo + "','" + Estado + "','" + Mensaje + "');";

                                SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                                cmd.ExecuteNonQuery();
                                ConexionBitacora.Dispose();
                                Environment.Exit(1);


                            }

                        }
                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {

                            ConexionBitacora.Open();
                           
                            string Estado = "ERROR";
                            string Mensaje = "No se encontro el Id Proporcionado en la Base de datos FEL";
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                            SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                            cmd.ExecuteNonQuery();
                            ConexionBitacora.Dispose();
                            Environment.Exit(1);
                        }
                    }
                    ConexionFEL.Close();
                }

            }
            catch (Exception e)
            {
                string StringConexionFEL= string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {

                    ConexionBitacora.Open();

                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un Error inesperado al comprobar la existencia del ID proporcinado. " ;
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();
                    Environment.Exit(1);
                }
            }
        }
        #endregion
        #region Existe el Documento de Anulacion
        public void ExisteElDocumentoAnulacion(ListaArgumentos Argumentos)
        {
            try
            {
                string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionFEL = new SqlConnection(StringConexionFEL))
                {
                    string QueryDatosCertificador = "select resultado,referencia from feel_anulaciones where id =" + Argumentos.Id_Documento + ";";

                    ConexionFEL.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryDatosCertificador, ConexionFEL);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosAnulacion = ds.Tables[0];
                    if (TablaDatosAnulacion.Rows.Count != 0)
                    {
                        
                        string? Resultado = string.Empty;
                        string? Referencia = string.Empty;


                        foreach (DataRow item in TablaDatosAnulacion.Rows)
                        {
                            Resultado = Convert.ToString(item[0]).Trim();
                            Referencia = Convert.ToString(item[1]).Trim();

                        }
                        if (Resultado == "PENDIENTE")
                        {
                            using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                            {
                                ActualizarFEL.Open();
                                string QueryDTEs = "update feel_anulaciones set resultado = 'EN PROCESO' WHERE id = " + Argumentos.Id_Documento + ";";

                                SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                                cmd.ExecuteNonQuery();
                                ActualizarFEL.Dispose();
                            }
                            XMLAnular oXMLAnular = new XMLAnular();
                            int id_Docto = Convert.ToInt32(Argumentos.Id_Documento);
                            oXMLAnular.EjecutorAnulaciones(id_Docto, EmpresaDatosFEl, Argumentos, Referencia);
                        }
                        else
                        {
                            using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                            {

                                ConexionBitacora.Open();
                                string Estado = "ERROR";
                                string Mensaje = "El Id Proporcionado tiene un resultado diferente a PENDIENTE y no podemos procesar su solicitud en un resultado diferente. ";
                                string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "Anulacion" + "','" + Estado + "','" + Mensaje + "');";

                                SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                                cmd.ExecuteNonQuery();
                                ConexionBitacora.Dispose();
                                Environment.Exit(1);


                            }
                        }
                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {

                            ConexionBitacora.Open();

                            string Estado = "ERROR";
                            string Mensaje = "No se encontro el Id Proporcionado en la Base de datos FEL";
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                            SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                            cmd.ExecuteNonQuery();
                            ConexionBitacora.Dispose();
                            Environment.Exit(1);
                        }
                    }
                    ConexionFEL.Close();
                }

            }
            catch (Exception e)
            {
                string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {

                    ConexionBitacora.Open();

                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un Error inesperado al comprobar la existencia del ID proporcinado. ";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();
                    Environment.Exit(1);
                }
            }
        }
        #endregion

    }
}
