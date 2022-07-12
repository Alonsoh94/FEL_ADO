using FEL_ADO.REPOSITORIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ADO.DTE_SAT.MODULOS
{
    public class FrasesDTE
    {
        public string? CodigoEscenario { get; set; }
        public string? TipoFrase { get; set; }
        public FrasesDTE()
        {

        }

        public void FRASESXML(XmlNode NDatosEmision, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                string QueryFrases = "select codigo_escenario, tipo_frase from feel_dtes_frases where dte_id = " + Id_Doc;
                using (SqlConnection ConexionFrases = new SqlConnection(StringConexionFEL))
                {
                    ConexionFrases.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryFrases, ConexionFrases);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosEmpresa = ds.Tables[0];

                    //List<FrasesDTE> FrasesDTEList = new List<FrasesDTE>();
                    if (TablaDatosEmpresa.Rows.Count != 0)
                    {
                        //****----- NODO FASES
                        XmlNode NFrases = XML.CreateElement("dte", "Frases", dte);
                        NDatosEmision.AppendChild(NFrases);
                        foreach (DataRow item in TablaDatosEmpresa.Rows)
                        {
                            XmlNode NFrase = XML.CreateElement("dte", "Frase", dte);
                            NFrases.AppendChild(NFrase);

                            XmlAttribute ACodigoEscenario = XML.CreateAttribute("CodigoEscenario");
                            ACodigoEscenario.Value = Convert.ToString(item[0]);
                            NFrase.Attributes.Append(ACodigoEscenario);

                            XmlAttribute ATipoFrase = XML.CreateAttribute("TipoFrase");
                            ATipoFrase.Value = Convert.ToString(item[1]);
                            NFrase.Attributes.Append(ATipoFrase);
                        }
                    }
                    else
                    {

                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se han encontrado Frases para el ID Proporcionado";
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

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un al crear el XML, especificamente en FRASES";
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
}
