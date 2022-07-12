using FEL_ADO.PROCESOS.INFILE;
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
    public class XMLAnular
    {

        #region CREAR XML ANULACION
        ListaArgumentos? Argumentos;
        XmlDocument? XMLDocumentAnulacion;
       Empresa? EmpresaDatosFEL;
        Empresa? FelData;
        int id_Docto;
        string? TipoFactura;
        string? Referencia;
        string? NitEmisor;


        public void EjecutorAnulaciones(int IdFel,  Empresa DatosFEL, ListaArgumentos Argumentos1, string TipoFac)
        {
            FelData = DatosFEL;
            EmpresaDatosFEL = DatosFEL;
            id_Docto = IdFel;
            Argumentos = Argumentos1;
            TipoFactura = TipoFac;
           
            XmlDocument XmlAnulacion = new XmlDocument();

            string dte = "http://www.sat.gob.gt/dte/fel/0.1.0";
            string xsi = "http://www.w3.org/2001/XMLSchema-instance";
            string ds = "http://www.w3.org/2000/09/xmldsig#";
            string n1 = "http://www.altova.com/samplexml/other-namespace";
            string SL = @"http://www.sat.gob.gt/dte/fel/0.1.0 C:\Users\User\Desktop\FEL\Esquemas\GT_AnulacionDocumento-0.1.0.xsd";

            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);
            try
            {
                using (SqlConnection ConexionFCAM = new SqlConnection(StringConexionFEL))
                {
                    string QueryCambiaria = "select * From feel_anulaciones where id = " + Id_Doc;

                    ConexionFCAM.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryCambiaria, ConexionFCAM);
                    DataSet dset = new DataSet();
                    adaptador.Fill(dset);
                    DataTable TablaAnular = dset.Tables[0];

                    if (TablaAnular.Rows.Count != 0)
                    {
                        foreach (DataRow item in TablaAnular.Rows)
                        {
                            XmlNode GTAnulacionDocumento = XmlAnulacion.CreateElement("dte", "GTAnulacionDocumento", dte);
                            XmlAnulacion.AppendChild(GTAnulacionDocumento);

                            XmlDeclaration Declaraciones;
                            Declaraciones = XmlAnulacion.CreateXmlDeclaration("1.0", null, null);
                            Declaraciones.Encoding = "UTF-8";

                            XmlAnulacion.InsertBefore(Declaraciones, GTAnulacionDocumento);

                            //Creacion de Atributos al NodoGTDocumento xmlns:ds xmlns:dte xmlns:xsi Version  xsi: schemaLocation
                            XmlAttribute xmlnsds = XmlAnulacion.CreateAttribute("xmlns:ds");
                            xmlnsds.Value = ds;
                            GTAnulacionDocumento.Attributes.Append(xmlnsds);

                            XmlAttribute xmlnsdte = XmlAnulacion.CreateAttribute("xmlns:dte");
                            xmlnsdte.Value = dte;
                            GTAnulacionDocumento.Attributes.Append(xmlnsdte);

                            XmlAttribute xmlnsn1 = XmlAnulacion.CreateAttribute("xmlns:n1");
                            xmlnsn1.Value = n1;
                            GTAnulacionDocumento.Attributes.Append(xmlnsn1);

                            XmlAttribute xmlnsxsi = XmlAnulacion.CreateAttribute("xmlns:xsi");
                            xmlnsxsi.Value = xsi;
                            GTAnulacionDocumento.Attributes.Append(xmlnsxsi);

                            XmlAttribute Version = XmlAnulacion.CreateAttribute("Version");
                            Version.Value = "0.1";
                            GTAnulacionDocumento.Attributes.Append(Version);

                            XmlAttribute xsischemaLocation = XmlAnulacion.CreateAttribute("xsi", "schemaLocation", xsi);
                            xsischemaLocation.Value = SL;
                            GTAnulacionDocumento.Attributes.Append(xsischemaLocation);
                            // Fin de Atributos NodoGTDocumento

                            //NODO SAT
                            XmlNode NSAT = XmlAnulacion.CreateElement("dte", "SAT", dte);
                            GTAnulacionDocumento.AppendChild(NSAT);
                            // NOdo AnulacionDTE
                            XmlNode NAnulacionDTE = XmlAnulacion.CreateElement("dte", "AnulacionDTE", dte);
                            NSAT.AppendChild(NAnulacionDTE);

                            XmlAttribute ADatosCertificados = XmlAnulacion.CreateAttribute("ID");
                            ADatosCertificados.Value = "DatosCertificados";
                            NAnulacionDTE.Attributes.Append(ADatosCertificados);

                            // NOdo DatosGenerales
                            XmlNode NDatosGenerales = XmlAnulacion.CreateElement("dte", "DatosGenerales", dte);
                            NAnulacionDTE.AppendChild(NDatosGenerales);
                            //NDatosGenerales.InnerText = String.Empty;

                            Referencia = Convert.ToString(item[2]);
                         

                            //atributos Datos Generales
                            string EFechaHoraActual = Convert.ToDateTime(item[5]).ToString("yyyy-MM-dd");
                            string EZonahoraria = "T00:00:00.000-06:00";
                            string EFechaHOraCompleta = EFechaHoraActual + EZonahoraria;
                            //string FechaHora = oDGA.fecha_emision_documento.ToString("yyyy-MM-ddTHH:mm:ss.fffzz:ff");
                            XmlAttribute NFechaEmisionDocumentoAnular = XmlAnulacion.CreateAttribute("FechaEmisionDocumentoAnular");
                            NFechaEmisionDocumentoAnular.Value = EFechaHOraCompleta;
                            NDatosGenerales.Attributes.Append(NFechaEmisionDocumentoAnular);



                            string FechaHoraActual = Convert.ToDateTime(item[6]).ToString("yyyy-MM-dd");
                            string Zonahoraria = "T00:00:00.000-06:00";
                            string FechaHOraCompleta = FechaHoraActual + Zonahoraria;

                            //string FechaHora2 = oDGA.fecha_hora_anulacion.ToString("yyyy-MM-ddTHH:mm:ss.fffzz:ff");
                            XmlAttribute NFechaHoraAnulacion = XmlAnulacion.CreateAttribute("FechaHoraAnulacion");
                            NFechaHoraAnulacion.Value = FechaHOraCompleta;
                            NDatosGenerales.Attributes.Append(NFechaHoraAnulacion);

                            XmlAttribute AID = XmlAnulacion.CreateAttribute("ID");
                            AID.Value = "DatosAnulacion";
                            NDatosGenerales.Attributes.Append(AID);

                            XmlAttribute AIDReceptor = XmlAnulacion.CreateAttribute("IDReceptor");
                            AIDReceptor.Value = Convert.ToString(item[7]);
                            NDatosGenerales.Attributes.Append(AIDReceptor);


                            XmlAttribute AMotivoAnulacion = XmlAnulacion.CreateAttribute("MotivoAnulacion");
                            AMotivoAnulacion.Value = Convert.ToString(item[9]);
                            NDatosGenerales.Attributes.Append(AMotivoAnulacion);

                            XmlAttribute ANITEmisor = XmlAnulacion.CreateAttribute("NITEmisor");
                            ANITEmisor.Value = Convert.ToString(item[8]);
                            NDatosGenerales.Attributes.Append(ANITEmisor);

                            NitEmisor = Convert.ToString(item[8]);

                            XmlAttribute ANumeroDocumentoAAnular = XmlAnulacion.CreateAttribute("NumeroDocumentoAAnular");
                            ANumeroDocumentoAAnular.Value = Convert.ToString(item[10]);
                            NDatosGenerales.Attributes.Append(ANumeroDocumentoAAnular);

                        }
                        


                        // Acciones
                        XMLDocumentAnulacion = XmlAnulacion;

                        ProcesorXML();

                    }
                }
                }
            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error, especificamente al intentar guardar el xml de Anulacion en disco o al intentar guardarlo en xml_resultado de la base de datos fel";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();

                }
                using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                {
                    ActualizarFEL.Open();
                    string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                    SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                    cmd.ExecuteNonQuery();
                    ActualizarFEL.Dispose();
                }
                Environment.Exit(1);
            }

            

            
        }

        public void ProcesorXML()
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            string XMLB64 = string.Empty;
            try
            {
                StringWriter EscribirArchivo = new StringWriter();
                XmlTextWriter EscribirTextoArchivo = new XmlTextWriter(EscribirArchivo);
                XMLDocumentAnulacion.WriteTo(EscribirTextoArchivo);

                string ResultadoXMLGenerado = EscribirArchivo.ToString();
                if (EmpresaDatosFEL.Path_XML.Trim() == null || EmpresaDatosFEL.Path_XML.Trim() == string.Empty)
                {
                    using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                    {
                        ConexionBitacora.Open();
                        string Estado = "ADVERTENCIA";
                        string Mensaje = "No se ha proporcionado una ruta para guardar el XML";
                        string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                        SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                        cmd.ExecuteNonQuery();
                        ConexionBitacora.Dispose();
                    }
                }
                else
                {
                    XMLDocumentAnulacion.Save(EmpresaDatosFEL.Path_XML.Trim() + @"\AnulacionXML.xml");
                    using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                    {
                        ActualizarFEL.Open();
                        string QueryDTEs = "update feel_anulaciones set xml_resultado = '" + ResultadoXMLGenerado + "' WHERE id = " + Argumentos.Id_Documento + ";";

                        SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                        cmd.ExecuteNonQuery();
                        ActualizarFEL.Dispose();
                    }
                }

                byte[] toEncodeAsBytes = UTF8Encoding.UTF8.GetBytes(ResultadoXMLGenerado);
                string XMLbase64 = Convert.ToBase64String(toEncodeAsBytes);
                XMLB64 = XMLbase64;
                //Console.WriteLine("HASTA AQUI TODO CON LA ANULACION");
            }
            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error, especificamente al intentar guardar el xml de Anulacion en disco o al intentar guardarlo en xml_resultado de la base de datos fel";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();

                }
                using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                {
                    ActualizarFEL.Open();
                    string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                    SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                    cmd.ExecuteNonQuery();
                    ActualizarFEL.Dispose();
                }
                Environment.Exit(1);
            }


            try
            {
                if (EmpresaDatosFEL.fel_token.Trim() == null || XMLB64 == null || EmpresaDatosFEL.fel_usuario.Trim() == null)
                {
                    using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                    {
                        ConexionBitacora.Open();
                        string Estado = "ERROR";
                        string Mensaje = "Los datos de: Token, XMLBase64 o UsuarioFEL no pueden ser nulos o estar vacios.";
                        string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                        SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                        cmd.ExecuteNonQuery();
                        ConexionBitacora.Dispose();

                    }
                    using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                    {
                        ActualizarFEL.Open();
                        string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR', estado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

                        SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                        cmd.ExecuteNonQuery();
                        ActualizarFEL.Dispose();
                    }
                    Environment.Exit(1);

                }
                else
                {


                    //OBJETO ANULACION
                    Firmar ObjFirmarAnulacion = new Firmar
                    {
                        llave = FelData.fel_token.Trim(), // LA FIRMA UTILIZA EL TOKEN Y LA CERTIFICACION O ANULACION LA LLAEVE
                        archivo = XMLB64,
                        codigo = Referencia,
                        alias = FelData.fel_usuario.Trim(),
                        es_anulacion = "S"
                    };

                    Firmar oFirmar = new Firmar();
                    oFirmar.FirmarDocumento(ObjFirmarAnulacion, EmpresaDatosFEL, Argumentos, TipoFactura);
                   // Console.WriteLine("Anularemos el Documento");


                    try
                    {
                        
                        Anular ObjAnular = new Anular()  //Creacion del Objeto Anular
                        {
                            nit_emisor = NitEmisor,
                            correo_copia = "",
                            xml_dte = oFirmar.ObtenerXMLBase64Firmado()
                        };

                        //    ANULAR
                        Anular oAnular = new Anular();
                        oAnular.AnularDocumento(ObjAnular, EmpresaDatosFEL, Argumentos, Referencia);
                        
                        Environment.Exit(0); 
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception e)
            {
               // string DataRecibida = Convert.ToString(DatosRecibidosAnulacion);
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar Anular el Documento";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "Anulacion" + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();

                }
                using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                {
                    ActualizarFEL.Open();
                    string QueryDTEs = "update feel_anulaciones set resultado = 'ERROR', estado = 'ERROR' WHERE id = " + Argumentos.Id_Documento + ";";

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
