using FEL_ADO.REPOSITORIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ADO.DTE_SAT.COMPLEMENTOS
{
    public class FCAMComponenteDTE
    {
        #region XML FCAM
        public void COMPLEMENTOFACTURACAMBIARIAXML(XmlNode NComplementos, XmlDocument oXML, string dte, string xsi,ListaArgumentos Argumentos, string TipoFactura)
        {
           
            string cfc = "http://www.sat.gob.gt/dte/fel/CompCambiaria/0.1.0";

            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                using (SqlConnection ConexionFCAM = new SqlConnection(StringConexionFEL))
                {
                    string QueryCambiaria = "select * From feel_dtes_cambiarias where dte_id = " + Id_Doc;

                    ConexionFCAM.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryCambiaria, ConexionFCAM);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaFCAMComponenteDTE = ds.Tables[0];

                    if (TablaFCAMComponenteDTE.Rows.Count != 0)
                    {
                        foreach (DataRow item in TablaFCAMComponenteDTE.Rows)
                        {
                            XmlNode NodoComplemento = oXML.CreateElement("dte", "Complemento", dte);
                            NComplementos.AppendChild(NodoComplemento);

                            XmlAttribute AIDComplemento = oXML.CreateAttribute("IDComplemento");
                            AIDComplemento.Value = Convert.ToString(item[2]).Trim();
                            NodoComplemento.Attributes.Append(AIDComplemento);

                            XmlAttribute ANombreComplemento = oXML.CreateAttribute("NombreComplemento");
                            ANombreComplemento.Value = Convert.ToString(item[3]).Trim();
                            NodoComplemento.Attributes.Append(ANombreComplemento);

                            XmlAttribute URIComplemento = oXML.CreateAttribute("URIComplemento");
                            URIComplemento.Value = Convert.ToString(item[4]).Trim();
                            NodoComplemento.Attributes.Append(URIComplemento);

                            //NODO ABONO FACTURA CAMBIARIA

                            XmlNode NAbonosFacturaCambiaria = oXML.CreateElement("cfc", "AbonosFacturaCambiaria", "cfc");
                            NodoComplemento.AppendChild(NAbonosFacturaCambiaria);

                            XmlAttribute Nxmlnscfc = oXML.CreateAttribute("xmlns:cfc");
                            Nxmlnscfc.Value = cfc;
                            NAbonosFacturaCambiaria.Attributes.Append(Nxmlnscfc);

                            XmlAttribute NVersionFCAM = oXML.CreateAttribute("Version");
                            NVersionFCAM.Value = "1";
                            NAbonosFacturaCambiaria.Attributes.Append(NVersionFCAM);


                            XmlAttribute AschemaLocation = oXML.CreateAttribute("xsi", "schemaLocation", xsi);
                            AschemaLocation.Value = @"http://www.sat.gob.gt/dte/fel/CompCambiaria/0.1.0 C:\Users\FEL\Desktop\Esquemas\GT_Complemento_Cambiaria-0.1.0.xsd";
                            NAbonosFacturaCambiaria.Attributes.Append(AschemaLocation);

                            //NODO ABONO
                            XmlNode NAbono = oXML.CreateElement("cfc", "Abono", cfc);
                            NAbonosFacturaCambiaria.AppendChild(NAbono);

                            XmlNode NNumeroAbono = oXML.CreateElement("cfc", "NumeroAbono", cfc);  //NODO Numero ABONO
                            NAbono.AppendChild(NNumeroAbono);
                            NNumeroAbono.InnerText = Convert.ToString(item[5]).Trim();
                            
                            XmlNode NFechaVencimiento = oXML.CreateElement("cfc", "FechaVencimiento", cfc);  //NODO Fecha Vencimiento
                            NAbono.AppendChild(NFechaVencimiento);
                            NFechaVencimiento.InnerText = Convert.ToDateTime(item[6]).ToString("yyyy-MM-dd"); // Convert.ToString(item.FechaVencimiento.ToString("yyyy-MM-dd"));

                            XmlNode NMontoAbono = oXML.CreateElement("cfc", "MontoAbono", cfc);  //NODO Numero ABONO
                            NAbono.AppendChild(NMontoAbono);
                            NMontoAbono.InnerText = Convert.ToString(item[7]);
                        }
                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se encontraron datos del complementos de FCAM ";
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

                    ConexionFCAM.Close();

                }
            }
            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "No se encontraron datos del complementos de FCAM ";
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


            #endregion
        }

    }
}
