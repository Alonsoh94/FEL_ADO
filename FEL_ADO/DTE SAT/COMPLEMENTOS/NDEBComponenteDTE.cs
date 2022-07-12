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
    public class NDEBComponenteDTE
    {

        public void COMPLEMENTONDEBXML(XmlNode NComplementos, XmlDocument oXML, string dte, string xsi, ListaArgumentos Argumentos, string TipoFactura)
        {
            string cno = "http://www.sat.gob.gt/face2/ComplementoReferenciaNota/0.1.0";
            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                using (SqlConnection ConexionFCAM = new SqlConnection(StringConexionFEL))
                {
                    string QueryCambiaria = "select * From feel_dtes_notas where dte_id = " + Id_Doc;

                    ConexionFCAM.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryCambiaria, ConexionFCAM);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaNCRE = ds.Tables[0];

                    if (TablaNCRE.Rows.Count != 0)
                    {
                        foreach (DataRow item in TablaNCRE.Rows)
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

                            //NODO REFERENCIA NOTA
                            XmlNode NReferenciasNota = oXML.CreateElement("cno", "ReferenciasNota", cno);  // nodo Gran Total
                            NodoComplemento.AppendChild(NReferenciasNota);

                            XmlAttribute Axmlnscno = oXML.CreateAttribute("xmlns:cno");
                            Axmlnscno.Value = cno;
                            NReferenciasNota.Attributes.Append(Axmlnscno);

                            XmlAttribute AFechaEmisionOrigen = oXML.CreateAttribute("FechaEmisionDocumentoOrigen");
                            AFechaEmisionOrigen.Value = Convert.ToDateTime(item[7]).ToString("yyyy-MM-dd");
                            NReferenciasNota.Attributes.Append(AFechaEmisionOrigen);

                            XmlAttribute AMotivoAjuste = oXML.CreateAttribute("MotivoAjuste");
                            AMotivoAjuste.Value = Convert.ToString(item[8]).Trim();
                            NReferenciasNota.Attributes.Append(AMotivoAjuste);

                            XmlAttribute ANumeroAutorizacionDocumentoOrigen = oXML.CreateAttribute("NumeroAutorizacionDocumentoOrigen");
                            ANumeroAutorizacionDocumentoOrigen.Value = Convert.ToString(item[6]).Trim();
                            NReferenciasNota.Attributes.Append(ANumeroAutorizacionDocumentoOrigen);

                            XmlAttribute ANumeroDocumentoOrigen = oXML.CreateAttribute("NumeroDocumentoOrigen");
                            ANumeroDocumentoOrigen.Value = Convert.ToString(item[10]).Trim();
                            NReferenciasNota.Attributes.Append(ANumeroDocumentoOrigen);

                            XmlAttribute AVersion = oXML.CreateAttribute("Version");
                            AVersion.Value = "0.0";
                            NReferenciasNota.Attributes.Append(AVersion);

                            XmlAttribute AxsischemaLocation = oXML.CreateAttribute("xsi", "schemaLocation", xsi);
                            AxsischemaLocation.Value = @"http://www.sat.gob.gt/face2/ComplementoReferenciaNota/0.1.0 C:\Users\User\Desktop\FEL\Esquemas\GT_Complemento_Referencia_Nota-0.1.0.xsd";
                            NReferenciasNota.Attributes.Append(AxsischemaLocation);

                        }

                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se encontraron datos del complementos de NCRE ";
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
                    string Mensaje = "Ha Ocurrido un error al intentar crear el XML complemento de NCRE ";
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
