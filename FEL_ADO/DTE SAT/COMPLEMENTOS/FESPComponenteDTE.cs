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
    public class FESPComponenteDTE
    {
        public void COMPLEMENTOFACTURAESPECIALXML(XmlNode NComplementos, XmlDocument oXML, string dte, string xsi,ListaArgumentos Argumentos, string TipoFactura)
        {
           
            string cfe = "http://www.sat.gob.gt/face2/ComplementoFacturaEspecial/0.1.0";

            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                using (SqlConnection ConexionFCAM = new SqlConnection(StringConexionFEL))
                {
                    string QueryCambiaria = "select * From feel_dtes_especial where dte_id = " + Id_Doc;

                    ConexionFCAM.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryCambiaria, ConexionFCAM);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaFESP = ds.Tables[0];

                    if (TablaFESP.Rows.Count != 0)
                    {
                        foreach (DataRow item in TablaFESP.Rows)
                        {
                            XmlNode NodoComplemento = oXML.CreateElement("dte", "Complemento", dte);
                            NComplementos.AppendChild(NodoComplemento);

                            XmlAttribute AIDComplemento = oXML.CreateAttribute("IDComplemento");
                            AIDComplemento.Value = Convert.ToString(item[2]);
                            NodoComplemento.Attributes.Append(AIDComplemento);

                            XmlAttribute ANombreComplemento = oXML.CreateAttribute("NombreComplemento");
                            ANombreComplemento.Value = Convert.ToString(item[3]);
                            NodoComplemento.Attributes.Append(ANombreComplemento);

                            XmlAttribute URIComplemento = oXML.CreateAttribute("URIComplemento");
                            URIComplemento.Value = Convert.ToString(item[4]);
                            NodoComplemento.Attributes.Append(URIComplemento);

                            //NODO ABONO FACTURA CAMBIARIA

                            XmlNode NRetencionesFacturaEspecial = oXML.CreateElement("cfe", "RetencionesFacturaEspecial", "cfe");
                            NodoComplemento.AppendChild(NRetencionesFacturaEspecial);

                            XmlAttribute Nxmlnscfc = oXML.CreateAttribute("xmlns:cfe");
                            Nxmlnscfc.Value = cfe;
                            NRetencionesFacturaEspecial.Attributes.Append(Nxmlnscfc);

                            XmlAttribute NVersionFEsp = oXML.CreateAttribute("Version");
                            NVersionFEsp.Value = "1";
                            NRetencionesFacturaEspecial.Attributes.Append(NVersionFEsp);


                            XmlAttribute AschemaLocationEsp = oXML.CreateAttribute("xsi", "schemaLocation", xsi);
                            AschemaLocationEsp.Value = @"http://www.sat.gob.gt/face2/ComplementoFacturaEspecial/0.1.0 C:\Users\User\Desktop\FEL\Esquemas\GT_Complemento_Fac_Especial-0.1.0.xsd";
                            NRetencionesFacturaEspecial.Attributes.Append(AschemaLocationEsp);

                            //NODO ABONO
                            XmlNode NRetencionISR = oXML.CreateElement("cfe", "RetencionISR", cfe); // Nodo ISR
                            NRetencionesFacturaEspecial.AppendChild(NRetencionISR);
                            NRetencionISR.InnerText = Convert.ToString(item[5]);

                            XmlNode NRetencionIVA = oXML.CreateElement("cfe", "RetencionIVA", cfe);  //NODO Iva
                            NRetencionesFacturaEspecial.AppendChild(NRetencionIVA);
                            NRetencionIVA.InnerText = Convert.ToString(item[6]);

                            XmlNode NTotalMenosRetenciones = oXML.CreateElement("cfe", "TotalMenosRetenciones", cfe);  //NODO Total
                            NRetencionesFacturaEspecial.AppendChild(NTotalMenosRetenciones);
                            NTotalMenosRetenciones.InnerText = Convert.ToString(item[7]);

                        }

                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se encontraron datos del complementos de FESP ";
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
                    string Mensaje = "Ocurrio un error al crear el XML complementos de FCAM ";
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
