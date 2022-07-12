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
    public class ImpuestosDTE
    {
        public string? NOmbreCorto { get; set; }
        public decimal TotalMontoImpuestos { get; set; }

        public ImpuestosDTE()
        {

        }

        public void IMPUESTOXML(XmlNode NTotales, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                

                using (SqlConnection ConexionItems = new SqlConnection(StringConexionFEL))
                {
                    string QueryImpuestos = "select * From feel_dtes_impuestos where dte_id = " + Id_Doc;

                    ConexionItems.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryImpuestos, ConexionItems);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosItems = ds.Tables[0];

                    if (TablaDatosItems.Rows.Count != 0)
                    {
                        foreach (DataRow Impuesto in TablaDatosItems.Rows)
                        {

                            XmlNode NTotalImpuestos = XML.CreateElement("dte", "TotalImpuestos", dte);  // nodo Totales impusto
                            NTotales.AppendChild(NTotalImpuestos);

                            XmlNode NTotalImpuesto = XML.CreateElement("dte", "TotalImpuesto", dte);  // nodo Totales impusto
                            NTotalImpuestos.AppendChild(NTotalImpuesto);

                            XmlAttribute ANombreCorto = XML.CreateAttribute("NombreCorto");
                            ANombreCorto.Value = Convert.ToString(Impuesto[2]); //Definir
                            NTotalImpuesto.Attributes.Append(ANombreCorto);

                            XmlAttribute ATotalMontoImpuesto = XML.CreateAttribute("TotalMontoImpuesto");
                            ATotalMontoImpuesto.Value = Convert.ToString(Impuesto[3]); //Definir
                            NTotalImpuesto.Attributes.Append(ATotalMontoImpuesto);
                        }
                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se han encontrado datos de Impuestos";
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
            catch (Exception )
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un al crear el XML, especificamente en IMPUESTOS";
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
