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
    public class ComponenteExportacionDTE
    {
        public void COMPLEMENTOSEXPORTACIONXML(XmlNode NComplementos, XmlDocument oXML, string dte, string xsi, ListaArgumentos Argumentos, string TipoFactura)
        {
            string cex = "http://www.sat.gob.gt/face2/ComplementoExportaciones/0.1.0";

            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                using (SqlConnection ConexionItems = new SqlConnection(StringConexionFEL))
                {
                    string QueryImpuestos = "select * From feel_dtes_exportaciones where dte_id = " + Id_Doc;

                    ConexionItems.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryImpuestos, ConexionItems);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaComponenteExportacionDTE = ds.Tables[0];

                    if(TablaComponenteExportacionDTE.Rows.Count != 0)
                    {
                        foreach (DataRow item in TablaComponenteExportacionDTE.Rows)
                        {

                            // NODO COMPLEMENTO
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

                            //NODO ABONO EXPORTACION
                            /* XmlNode NExportacion = oXML.CreateElement("cex", "Exportacion", "cex");
                             NodoComplemento.AppendChild(NExportacion); */
                            XmlNode NExportacion = oXML.CreateElement("cex", "Exportacion", cex);
                            NodoComplemento.AppendChild(NExportacion);


                            XmlAttribute NVersionExp = oXML.CreateAttribute("Version");
                            NVersionExp.Value = "1";
                            NExportacion.Attributes.Append(NVersionExp);

                            XmlAttribute AschemaLocation = oXML.CreateAttribute("xsi", "schemaLocation", xsi);
                            AschemaLocation.Value = @"http://www.sat.gob.gt/face2/ComplementoExportaciones/0.1.0 C:\Users\User\Desktop\FEL\Esquemas\GT_Complemento_Exportaciones-0.1.0.xsd";
                            NExportacion.Attributes.Append(AschemaLocation);

                            //NODOS EXPORTACIONES
                            XmlNode nNombreConsignatarioODestinatario = oXML.CreateElement("cex", "NombreConsignatarioODestinatario", cex);
                            NExportacion.AppendChild(nNombreConsignatarioODestinatario);
                            nNombreConsignatarioODestinatario.InnerText = Convert.ToString(item[5]).Trim();

                            XmlNode DireccionConsignatarioODestinatario = oXML.CreateElement("cex", "DireccionConsignatarioODestinatario", cex);
                            NExportacion.AppendChild(DireccionConsignatarioODestinatario);
                            DireccionConsignatarioODestinatario.InnerText = Convert.ToString(item[6]).Trim();

                            XmlNode CodigoConsignatarioODestinatario = oXML.CreateElement("cex", "CodigoConsignatarioODestinatario", cex);
                            NExportacion.AppendChild(CodigoConsignatarioODestinatario);
                            CodigoConsignatarioODestinatario.InnerText = Convert.ToString(item[7]).Trim();

                            XmlNode NombreComprador = oXML.CreateElement("cex", "NombreComprador", cex);
                            NExportacion.AppendChild(NombreComprador);
                            NombreComprador.InnerText = Convert.ToString(item[8]).Trim();

                            XmlNode DireccionComprador = oXML.CreateElement("cex", "DireccionComprador", cex);
                            NExportacion.AppendChild(DireccionComprador);
                            DireccionComprador.InnerText = Convert.ToString(item[9]).Trim();

                            XmlNode CodigoComprador = oXML.CreateElement("cex", "CodigoComprador", cex);
                            NExportacion.AppendChild(CodigoComprador);
                            CodigoComprador.InnerText = Convert.ToString(item[10]).Trim();

                            XmlNode OtraReferencia = oXML.CreateElement("cex", "OtraReferencia", cex);
                            NExportacion.AppendChild(OtraReferencia);
                            OtraReferencia.InnerText = Convert.ToString(item[11]).Trim();

                            XmlNode INCOTERM = oXML.CreateElement("cex", "INCOTERM", cex);
                            NExportacion.AppendChild(INCOTERM);
                            INCOTERM.InnerText = Convert.ToString(item[12]).Trim();

                            XmlNode NombreExportador = oXML.CreateElement("cex", "NombreExportador", cex);
                            NExportacion.AppendChild(NombreExportador);
                            NombreExportador.InnerText = Convert.ToString(item[13]).Trim();

                            XmlNode CodigoExportador = oXML.CreateElement("cex", "CodigoExportador", cex);
                            NExportacion.AppendChild(CodigoExportador);
                            CodigoExportador.InnerText = Convert.ToString(item[14]).Trim();
                        }

                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se han encontrados datos de exportación";
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
                    string Mensaje = "No se han encontrados datos de exportación";
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
