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
    public class ImpuestosItemDTE
    {

        public void IMPUESTOSITEMSXML(XmlNode NItem, dynamic XML, string dte, int Id_item, ListaArgumentos Argumentos, string TipoFactura)
        {
           
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            //int Id_Doc = Convert.ToInt32(Id_item);
            try
            {
                string QueryFrases = "select * From feel_items_impuestos where item_id = " + Id_item;
                using (SqlConnection ConexionItems = new SqlConnection(StringConexionFEL))
                {
                    ConexionItems.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryFrases, ConexionItems);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaImpuestosItems = ds.Tables[0];

                    if(TablaImpuestosItems.Rows.Count != 0)
                    {
                        XmlNode NImpuestos = XML.CreateElement("dte", "Impuestos", dte);
                        NItem.AppendChild(NImpuestos);

                        foreach (DataRow Imp in TablaImpuestosItems.Rows)
                        {

                            XmlNode NImpuesto = XML.CreateElement("dte", "Impuesto", dte);
                            NImpuestos.AppendChild(NImpuesto);

                            XmlNode NNombreCorto = XML.CreateElement("dte", "NombreCorto", dte);
                            NImpuesto.AppendChild(NNombreCorto);
                            NNombreCorto.InnerText = Convert.ToString(Imp[2]).Trim();

                            XmlNode NCodigoUnidadGravable = XML.CreateElement("dte", "CodigoUnidadGravable", dte);
                            NImpuesto.AppendChild(NCodigoUnidadGravable);
                            NCodigoUnidadGravable.InnerText = Convert.ToString(Imp[3]);

                            XmlNode NMontoGravable = XML.CreateElement("dte", "MontoGravable", dte);
                            NImpuesto.AppendChild(NMontoGravable);
                            NMontoGravable.InnerText = Convert.ToString(Imp[4]);

                            XmlNode NMontoImpuesto = XML.CreateElement("dte", "MontoImpuesto", dte);
                            NImpuesto.AppendChild(NMontoImpuesto);
                            NMontoImpuesto.InnerText = Convert.ToString(Imp[6]);


                        }
                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se Encontraron datos en Impuestos Items";
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
