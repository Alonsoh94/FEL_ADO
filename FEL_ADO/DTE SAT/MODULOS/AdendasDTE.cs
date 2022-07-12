using FEL_ADO.REPOSITORIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ADO.DTE_SAT.XML
{
    public class AdendasDTE
    {
        public string? llave { get; set; }
        public string? valor { get; set; }

        public AdendasDTE()
        {

        }

        public void ADENDASXML(XmlNode NSAT, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {

                int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
                using (SqlConnection ConexionAdendas = new SqlConnection(StringConexionFEL))
                {
                    string QueryImpuestos = "select * From feel_dtes_adendas where dte_id = " + Id_Doc;

                    ConexionAdendas.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryImpuestos, ConexionAdendas);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosAdendas = ds.Tables[0];


                    if (TablaDatosAdendas.Rows.Count != 0)
                    {
                        XmlNode NodoAdenda = XML.CreateElement("dte", "Adenda", dte);
                        NSAT.AppendChild(NodoAdenda);

                        foreach (DataRow item in TablaDatosAdendas.Rows)
                        {

                            XmlNode NodoAdendaItem = XML.CreateElement(Convert.ToString(item[2]).Trim().ToLower());
                            NodoAdenda.AppendChild(NodoAdendaItem);
                            NodoAdendaItem.InnerText = Convert.ToString(item[3]).Trim();
                        }
                    }
                    
                    ConexionAdendas.Close();
                }
            
            }
            catch (Exception)
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error, especificamente en datos de Adendas";
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
