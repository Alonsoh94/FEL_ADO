using FEL_ADO.DTE_SAT.MODULOS;
using FEL_ADO.REPOSITORIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using FEL_ADO.PROCESOS.INFILE;
using FEL_ADO.DTE_SAT.COMPLEMENTOS;

namespace FEL_ADO.DTE_SAT.XML
{
    public class XMLCertificar
    {
        #region Declaracion de XML
        XmlDocument XML = new XmlDocument();
        XmlNode NodoRefDatosEmision;
        XmlNode NodoRefTotales;
        XmlNode NodoRefSAT;
        XmlNode NodoRefComplementos;

        Empresa EmpresaDatosFEL;
        ListaArgumentos Argumentos;


        string dte = "http://www.sat.gob.gt/dte/fel/0.2.0";
        string xsi = "http://www.w3.org/2001/XMLSchema-instance";
        string ds = "http://www.w3.org/2000/09/xmldsig#";
        #endregion


        public void EjecutorCertificaciones(ListaArgumentos ArgumentosList, Empresa EmpresaFEL, string Expo, string TipoFactura)
        {
            EmpresaDatosFEL = EmpresaFEL;
            Argumentos = ArgumentosList;
            switch (TipoFactura)
            {
                //FACTURAS
                case "FACT":
                   ConstruirXMLFACT();
                    break;
                case "FCAM": // FACTURA CAMBIARIA
                    if (Expo == "SI")
                    {
                       ConstruirXMLFACTEXPORTACION();
                    }
                    else
                    {
                       ConstriuirXMLFCAM();
                    }

                    break;

                case "FPEQ": //FACTURAS DE PEQUEÑO CONTRIBUYENTE

                    break;
                case "FCAP":

                    break;
                //FACTURAS ESPECIALES, NOTA DE ABONO Y RECIBO
                case "FESP":
                   ConstriuirXMLFESP();
                    break;
                case "NABN":
                    ConstruirXMLNABN();

                    break;
                case "RDON":

                    break;
                case "RECI":

                    break;
                //NOTAS DE CRÉDITO Y DÉBITO
                case "NDEB":
                  ConstruirXMLNDEB();
                    break;
                case "NCRE":
                  ConstruirXMLNCRE();
                    break;
                default:
                    Console.WriteLine("No hay Acronimos a el tipo de factura recibido");
                    break;
            }
        }

        #region Construir Cabece XML
        void CabeceraXML()
            {
                XmlNode NodoGTDocumento = XML.CreateElement("dte", "GTDocumento", dte);
                XML.AppendChild(NodoGTDocumento);

                XmlDeclaration Declaraciones;
                Declaraciones = XML.CreateXmlDeclaration("1.0", null, null);
                Declaraciones.Encoding = "UTF-8";
                XML.InsertBefore(Declaraciones, NodoGTDocumento);

                //Creacion de Atributos al NodoGTDocumento xmlns:ds xmlns:dte xmlns:xsi  Version  xsi:schemaLocation
                XmlAttribute xmlnsds = XML.CreateAttribute("xmlns:ds");
                xmlnsds.Value = ds;
                NodoGTDocumento.Attributes.Append(xmlnsds);

                XmlAttribute xmlnsdte = XML.CreateAttribute("xmlns:dte");
                xmlnsdte.Value = dte;
                NodoGTDocumento.Attributes.Append(xmlnsdte);

                XmlAttribute xmlnsxsi = XML.CreateAttribute("xmlns:xsi");
                xmlnsxsi.Value = xsi;
                NodoGTDocumento.Attributes.Append(xmlnsxsi);

                XmlAttribute Version = XML.CreateAttribute("Version");
                Version.Value = "0.1";
                NodoGTDocumento.Attributes.Append(Version);

                XmlAttribute xsischemaLocation = XML.CreateAttribute("xsi", "schemaLocation", xsi);
                xsischemaLocation.Value = "http://www.sat.gob.gt/dte/fel/0.2.0";
                NodoGTDocumento.Attributes.Append(xsischemaLocation);
                // Fin de Atributos NodoGTDocumento


                //NODO SAT
                XmlNode NSAT = XML.CreateElement("dte", "SAT", dte);
                NodoGTDocumento.AppendChild(NSAT);
                NodoRefSAT = NSAT;

                XmlAttribute ClaseDocumento = XML.CreateAttribute("ClaseDocumento");
                ClaseDocumento.Value = "dte";
                NSAT.Attributes.Append(ClaseDocumento);

                //NODO NDTE
                XmlNode NDTE = XML.CreateElement("dte", "DTE", dte);
                NSAT.AppendChild(NDTE);

                XmlAttribute ID = XML.CreateAttribute("ID");
                ID.Value = "DatosCertificados";
                NDTE.Attributes.Append(ID);

                // NODO NDTE __________

                // ****-----
                XmlNode NDatosEmision = XML.CreateElement("dte", "DatosEmision", dte);
                NDTE.AppendChild(NDatosEmision);

                XmlAttribute IDDE = XML.CreateAttribute("ID");
                IDDE.Value = "DatosEmision";
                NDatosEmision.Attributes.Append(IDDE);
                //----*****

                NodoRefDatosEmision = NDatosEmision;
            }
            #endregion

            #region CrearNodoTotales
            void NodoTotales()
            {
                XmlNode NTotales = XML.CreateElement("dte", "Totales", dte);  // nodo Totales ***********
                NodoRefDatosEmision.AppendChild(NTotales);
                NodoRefTotales = NTotales;
            }
            #endregion
         #region CrearNodoComplementos
         public void CrearNodoComplementos()
         {
             XmlNode NComplementos = XML.CreateElement("dte", "Complementos", dte);
             NodoRefDatosEmision.AppendChild(NComplementos);
             NodoRefComplementos = NComplementos;
         }
        #endregion
        #region Construccion de Datos no Variables
        EmisorDTE oEmisorDTE = new EmisorDTE();
        ReceptorDTE oReceptorDTE = new ReceptorDTE();
        DatosGeneralesDTE oDatosGenerales = new DatosGeneralesDTE();
        InfoRequerida oInfoRequerida = new InfoRequerida();
        public decimal GranTotalDTE;
        public void DatosNoVariables()
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {


                using (SqlConnection ConexionDTE = new SqlConnection(StringConexionFEL))
                {

                    string QueryDatosCertificador = " select * from feel_dtes where id =" + Argumentos.Id_Documento + ";";

                    ConexionDTE.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryDatosCertificador, ConexionDTE);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosDTE = ds.Tables[0];
                    if(TablaDatosDTE.Rows.Count != 0)
                    {

                        foreach (DataRow item in TablaDatosDTE.Rows)
                        {
                            oInfoRequerida.Id = Convert.ToInt32(item[0]);
                            oInfoRequerida.Referencia = Convert.ToString(item[1]).Trim();
                            oInfoRequerida.Resultado = Convert.ToString(item[2]).Trim();
                            oInfoRequerida.XLM_Resultado = Convert.ToString(item[3]).Trim();  
                            oDatosGenerales.FechaHoraEmision = Convert.ToDateTime(item[4]);
                            oDatosGenerales.NumeroAcceso = Convert.ToInt32(item[5]);
                            oDatosGenerales.CodigoMoneda = Convert.ToString(item[6]).Trim();
                            oDatosGenerales.Tipo = Convert.ToString(item[7]).Trim();
                            oDatosGenerales.Exportacion = Convert.ToString(item[8]).Trim();
                            oEmisorDTE.Correo = Convert.ToString(item[9]).Trim();
                            oEmisorDTE.Establecimiento = Convert.ToInt32(item[10]);
                            oEmisorDTE.Nit = Convert.ToString(item[11]).Trim();
                            oEmisorDTE.NombreComercial = Convert.ToString(item[12]).Trim();
                            oEmisorDTE.AfiliacionIva = Convert.ToString(item[13]).Trim();
                            oEmisorDTE.NombreEmisor = Convert.ToString(item[14]).Trim();
                            oEmisorDTE.Direccion = Convert.ToString(item[15]).Trim();
                            oEmisorDTE.CodigoPostal = Convert.ToInt32(item[16]);
                            oEmisorDTE.Municipio = Convert.ToString(item[17]).Trim();
                            oEmisorDTE.Departamento = Convert.ToString(item[18]).Trim();
                            oEmisorDTE.Pais = Convert.ToString(item[19]).Trim();
                            oReceptorDTE.IDReceptor = Convert.ToString(item[20]).Trim();
                            oReceptorDTE.Correo = Convert.ToString(item[21]).Trim();
                            oReceptorDTE.NombreReceptor = Convert.ToString(item[22]).Trim();
                            oReceptorDTE.Direccion = Convert.ToString(item[23]).Trim();
                            oReceptorDTE.CodigoPostal = Convert.ToInt32(item[24]);
                            oReceptorDTE.Municipio = Convert.ToString(item[25]).Trim();
                            oReceptorDTE.Departamento = Convert.ToString(item[26]).Trim();
                            oReceptorDTE.Pais = Convert.ToString(item[27]).Trim();
                            oReceptorDTE.drType = Convert.ToString(item[28]).Trim();
                            GranTotalDTE = Convert.ToDecimal(item[29]);
                            oInfoRequerida.uuid = Convert.ToString(item[30]).Trim();
                            oInfoRequerida.Numero = Convert.ToString(item[31]).Trim();
                            oInfoRequerida.Serie = Convert.ToString(item[32]).Trim();
                            oInfoRequerida.Tipo_Personeria = Convert.ToString(item[33]).Trim();

                        }

                      //  Console.WriteLine("Todos los Datos Leidos");

                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "Se ha producido un error intentar leer los datos del Id proporcionado. Veifique que todos sus datos esten correos e intente de nuevo.";
                            string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

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
                   


                    ConexionDTE.Close();
                }
                    

            }
            catch (Exception)
            {


                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error inesperado al intentar leer los datos del Id proporcionado. Veifique que todos sus datos esten correos e intente de nuevo.";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + "N/A" + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion

      

        #region Construir XML FACT
        public void ConstruirXMLFACT()
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
                FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
                ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();                
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                  NodoTotales();
                InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();
                  

                
            }
            catch (Exception ex)
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de FACT ";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion
        #region CONSTRUIR XML PARA FCAM
        public void ConstriuirXMLFCAM() // lleva todo lo de fact + complemento de cambiarias.
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
                FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
                ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                FCAMComponenteDTE InstFcam = new FCAMComponenteDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                NodoTotales();
                InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                CrearNodoComplementos();
                InstFcam.COMPLEMENTOFACTURACAMBIARIAXML(NodoRefComplementos, XML, dte, xsi, Argumentos, oDatosGenerales.Tipo);

                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();

            }
            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de FCAM ";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion
        #region Construir XML FACT EXPORTACION
        public void ConstruirXMLFACTEXPORTACION()
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
                FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
                ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                FCAMComponenteDTE InstFcam = new FCAMComponenteDTE();
                ComponenteExportacionDTE InsEXPO = new ComponenteExportacionDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                NodoTotales();
                InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                CrearNodoComplementos();
                InstFcam.COMPLEMENTOFACTURACAMBIARIAXML(NodoRefComplementos, XML, dte, xsi, Argumentos, oDatosGenerales.Tipo);
                InsEXPO.COMPLEMENTOSEXPORTACIONXML(NodoRefComplementos, XML, dte, xsi, Argumentos, oDatosGenerales.Tipo);
                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();               
            }
            catch (Exception ex)
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de FCAM con complementos de Exportación";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion
        #region CONSTRUIR XML PARA FESP
        public void ConstriuirXMLFESP() // NO lleva Frases
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
                //FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
                ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                FESPComponenteDTE InstFesp = new FESPComponenteDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                //oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                NodoTotales();
                InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                CrearNodoComplementos();
                InstFesp.COMPLEMENTOFACTURAESPECIALXML(NodoRefComplementos, XML, dte, xsi, Argumentos, oDatosGenerales.Tipo);

                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();

            }
            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de FESP";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion
        #region Construir XML NABN
        public void ConstruirXMLNABN()
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try  // ######### LAS NOTAS DE BONO NO LLEVAN COMPLEMENTOS.
            {

                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
               // FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
               // ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
               // oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                NodoTotales();
               // InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();                

            }
            catch (Exception ex)
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de NABN";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion

        #region Construir XML NCRE
        public void ConstruirXMLNCRE() // NO LLEVA FRASES
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
               // FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
                ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                NCREComponenteDTE oNCRE = new NCREComponenteDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
               // oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                NodoTotales();
                InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                CrearNodoComplementos();
                oNCRE.COMPLEMENTONCREXML(NodoRefComplementos, XML, dte, xsi,Argumentos, oDatosGenerales.Tipo);
                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();

                
            }
            catch (Exception ex)
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de FACT ";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion

        #region Construir XML NDEB
        public void ConstruirXMLNDEB() // NO LLEVA FRASES
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                EmisorDTE oEmisor = new EmisorDTE();
                DatosGeneralesDTE InsDatosGeneralesDTE = new DatosGeneralesDTE();
                ReceptorDTE InsReceptorDTE = new ReceptorDTE();
                // FrasesDTE oFrasesDTE = new FrasesDTE();
                ItemsDTE oItemsDTE = new ItemsDTE();
                ImpuestosDTE InsImpuestrosDTE = new ImpuestosDTE();
                TotalesDTE InsTotalesDTE = new TotalesDTE();
                NDEBComponenteDTE oDEB = new NDEBComponenteDTE();
                AdendasDTE InsAdendasDTE = new AdendasDTE();

                DatosNoVariables();
                CabeceraXML();
                InsDatosGeneralesDTE.DATOSGENERALESXML(oDatosGenerales, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oEmisor.DATAOSEMISORXML(oEmisorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsReceptorDTE.RECEPTORXML(oReceptorDTE, NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                // oFrasesDTE.FRASESXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                oItemsDTE.ITEMSXML(NodoRefDatosEmision, XML, dte, Argumentos, oDatosGenerales.Tipo);
                NodoTotales();
                InsImpuestrosDTE.IMPUESTOXML(NodoRefTotales, XML, dte, Argumentos, oDatosGenerales.Tipo);
                InsTotalesDTE.GRANTOTALXML(NodoRefTotales, XML, dte, GranTotalDTE, Argumentos, oDatosGenerales.Tipo);
                CrearNodoComplementos();
                oDEB.COMPLEMENTONDEBXML(NodoRefComplementos, XML, dte, xsi, Argumentos, oDatosGenerales.Tipo);
                InsAdendasDTE.ADENDASXML(NodoRefSAT, XML, dte, Argumentos, oDatosGenerales.Tipo);

                ProcesorXML();


            }
            catch (Exception ex)
            {
                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error al intentar consruir el XML de FACT ";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion

        #region PROCESO XML
        public async void ProcesorXML()
        {
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            string XMLB64 = string.Empty;
            try
            {
                StringWriter EscribirArchivo = new StringWriter();
                XmlTextWriter EscribirTextoArchivo = new XmlTextWriter(EscribirArchivo);
                XML.WriteTo(EscribirTextoArchivo);

                string ResultadoXMLGenerado = EscribirArchivo.ToString();
                if (EmpresaDatosFEL.Path_XML.Trim() == null || EmpresaDatosFEL.Path_XML.Trim() == string.Empty)
                {
                    using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                    {
                        ConexionBitacora.Open();
                        string Estado = "ADVERTENCIA";
                        string Mensaje = "No se ha proporcionado una ruta para guardar el XML";
                        string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

                        SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                        cmd.ExecuteNonQuery();
                        ConexionBitacora.Dispose();
                    }
                }
                else
                {
                    XML.Save(EmpresaDatosFEL.Path_XML.Trim() + @"\CertificacionXMLFACT.xml");                   
                    using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionFEL))
                    {
                        ActualizarFEL.Open();
                        string QueryDTEs = "update feel_dtes set xml_resultado = '" + ResultadoXMLGenerado + "' WHERE id = " + Argumentos.Id_Documento + ";";

                        SqlCommand cmd = new SqlCommand(QueryDTEs, ActualizarFEL);
                        cmd.ExecuteNonQuery();
                        ActualizarFEL.Dispose();
                    }
                }

                byte[] toEncodeAsBytes = UTF8Encoding.UTF8.GetBytes(ResultadoXMLGenerado);
                string XMLbase64 = Convert.ToBase64String(toEncodeAsBytes);
                XMLB64 = XMLbase64;
                //Console.WriteLine("HASTA AQUI TODO BIEN");
            }
            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error, especificamente al intentar guardar el xml en disco o al intentar guardarlo en xml_resultado de la base de datos fel";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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

            
            try
            {
                if (EmpresaDatosFEL.fel_token.Trim() == null || XMLB64 == null || oInfoRequerida.Referencia.Trim() == null || EmpresaDatosFEL.fel_usuario.Trim() == null)
                {
                    using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                    {
                        ConexionBitacora.Open();
                        string Estado = "ERROR";
                        string Mensaje = "No podemos construir el objeto para firmar, porque algunos de estos datos parece ser nulo: Token, Referencia, Usuario Fel, XMLBase64";
                        string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
                else
                {
                    //string Base64 = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz48ZHRlOkdURG9jdW1lbnRvIHhtbG5zOmRzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczpkdGU9Imh0dHA6Ly93d3cuc2F0LmdvYi5ndC9kdGUvZmVsLzAuMi4wIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiBWZXJzaW9uPSIwLjEiIHhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL3d3dy5zYXQuZ29iLmd0L2R0ZS9mZWwvMC4yLjAiPjxkdGU6U0FUIENsYXNlRG9jdW1lbnRvPSJkdGUiPjxkdGU6RFRFIElEPSJEYXRvc0NlcnRpZmljYWRvcyI+PGR0ZTpEYXRvc0VtaXNpb24gSUQ9IkRhdG9zRW1pc2lvbiI+PGR0ZTpEYXRvc0dlbmVyYWxlcyBDb2RpZ29Nb25lZGE9IkdUUSIgRmVjaGFIb3JhRW1pc2lvbj0iMjAyMi0wNi0yM1QwMDowMDowMC4wMDAtMDY6MDAiIFRpcG89IkZBQ1QiIC8+PGR0ZTpFbWlzb3IgQWZpbGlhY2lvbklWQT0iR0VOIiBDb2RpZ29Fc3RhYmxlY2ltaWVudG89IjEiIE5JVEVtaXNvcj0iMjk1NzkzMzMiIE5vbWJyZUNvbWVyY2lhbD0iU0FOIFBBVUwsIFMuQS4iIE5vbWJyZUVtaXNvcj0iU0FOIFBBVUwsIFMuQS4iPjxkdGU6RGlyZWNjaW9uRW1pc29yPjxkdGU6RGlyZWNjaW9uPkFWLiAgUEVUQVBBIDI1LTI1IFouMTI8L2R0ZTpEaXJlY2Npb24+PGR0ZTpDb2RpZ29Qb3N0YWw+MDwvZHRlOkNvZGlnb1Bvc3RhbD48ZHRlOk11bmljaXBpbz48L2R0ZTpNdW5pY2lwaW8+PGR0ZTpEZXBhcnRhbWVudG8+PC9kdGU6RGVwYXJ0YW1lbnRvPjxkdGU6UGFpcz5HVDwvZHRlOlBhaXM+PC9kdGU6RGlyZWNjaW9uRW1pc29yPjwvZHRlOkVtaXNvcj48ZHRlOlJlY2VwdG9yIENvcnJlb1JlY2VwdG9yPSJqb3NlLmFsb25zb0BzaWRndC5jb20iIElEUmVjZXB0b3I9Ijg1MjI1MzA0IiBOb21icmVSZWNlcHRvcj0iSk9TRSBBTE9OU08iPjxkdGU6RGlyZWNjaW9uUmVjZXB0b3I+PGR0ZTpEaXJlY2Npb24+Q0lVREFEPC9kdGU6RGlyZWNjaW9uPjxkdGU6Q29kaWdvUG9zdGFsPjA8L2R0ZTpDb2RpZ29Qb3N0YWw+PGR0ZTpNdW5pY2lwaW8+R1VBVEVNQUxBPC9kdGU6TXVuaWNpcGlvPjxkdGU6RGVwYXJ0YW1lbnRvPkdVQVRFTUFMQTwvZHRlOkRlcGFydGFtZW50bz48ZHRlOlBhaXM+R1Q8L2R0ZTpQYWlzPjwvZHRlOkRpcmVjY2lvblJlY2VwdG9yPjwvZHRlOlJlY2VwdG9yPjxkdGU6RnJhc2VzPjxkdGU6RnJhc2UgQ29kaWdvRXNjZW5hcmlvPSIxIiBUaXBvRnJhc2U9IjEiIC8+PC9kdGU6RnJhc2VzPjxkdGU6SXRlbXM+PGR0ZTpJdGVtIE51bWVyb0xpbmVhPSIxIiBCaWVuT1NlcnZpY2lvPSJCIj48ZHRlOkNhbnRpZGFkPjMuMDAwMDwvZHRlOkNhbnRpZGFkPjxkdGU6VW5pZGFkTWVkaWRhPlBBUjwvZHRlOlVuaWRhZE1lZGlkYT48ZHRlOkRlc2NyaXBjaW9uPkFUTEVUSUNPIEpVVkVOSUwgIFkgTklOTzwvZHRlOkRlc2NyaXBjaW9uPjxkdGU6UHJlY2lvVW5pdGFyaW8+MzUwLjAwMDA8L2R0ZTpQcmVjaW9Vbml0YXJpbz48ZHRlOlByZWNpbz4xMDUwLjAwPC9kdGU6UHJlY2lvPjxkdGU6RGVzY3VlbnRvPjAuMDA8L2R0ZTpEZXNjdWVudG8+PGR0ZTpJbXB1ZXN0b3M+PGR0ZTpJbXB1ZXN0bz48ZHRlOk5vbWJyZUNvcnRvPklWQTwvZHRlOk5vbWJyZUNvcnRvPjxkdGU6Q29kaWdvVW5pZGFkR3JhdmFibGU+MTwvZHRlOkNvZGlnb1VuaWRhZEdyYXZhYmxlPjxkdGU6TW9udG9HcmF2YWJsZT45MzcuNTA8L2R0ZTpNb250b0dyYXZhYmxlPjxkdGU6TW9udG9JbXB1ZXN0bz4xMTIuNTA8L2R0ZTpNb250b0ltcHVlc3RvPjwvZHRlOkltcHVlc3RvPjwvZHRlOkltcHVlc3Rvcz48ZHRlOlRvdGFsPjEwNTAuMDA8L2R0ZTpUb3RhbD48L2R0ZTpJdGVtPjxkdGU6SXRlbSBOdW1lcm9MaW5lYT0iMiIgQmllbk9TZXJ2aWNpbz0iQiI+PGR0ZTpDYW50aWRhZD4yLjAwMDA8L2R0ZTpDYW50aWRhZD48ZHRlOlVuaWRhZE1lZGlkYT5QQVI8L2R0ZTpVbmlkYWRNZWRpZGE+PGR0ZTpEZXNjcmlwY2lvbj5BVExFVElDTyBKVVZFTklMICBZIE5JTk88L2R0ZTpEZXNjcmlwY2lvbj48ZHRlOlByZWNpb1VuaXRhcmlvPjQwMC4wMDAwPC9kdGU6UHJlY2lvVW5pdGFyaW8+PGR0ZTpQcmVjaW8+ODAwLjAwPC9kdGU6UHJlY2lvPjxkdGU6RGVzY3VlbnRvPjAuMDA8L2R0ZTpEZXNjdWVudG8+PGR0ZTpJbXB1ZXN0b3M+PGR0ZTpJbXB1ZXN0bz48ZHRlOk5vbWJyZUNvcnRvPklWQTwvZHRlOk5vbWJyZUNvcnRvPjxkdGU6Q29kaWdvVW5pZGFkR3JhdmFibGU+MTwvZHRlOkNvZGlnb1VuaWRhZEdyYXZhYmxlPjxkdGU6TW9udG9HcmF2YWJsZT43MTQuMjk8L2R0ZTpNb250b0dyYXZhYmxlPjxkdGU6TW9udG9JbXB1ZXN0bz44NS43MTwvZHRlOk1vbnRvSW1wdWVzdG8+PC9kdGU6SW1wdWVzdG8+PC9kdGU6SW1wdWVzdG9zPjxkdGU6VG90YWw+ODAwLjAwPC9kdGU6VG90YWw+PC9kdGU6SXRlbT48ZHRlOkl0ZW0gTnVtZXJvTGluZWE9IjMiIEJpZW5PU2VydmljaW89IkIiPjxkdGU6Q2FudGlkYWQ+Ny4wMDAwPC9kdGU6Q2FudGlkYWQ+PGR0ZTpVbmlkYWRNZWRpZGE+VU5JPC9kdGU6VW5pZGFkTWVkaWRhPjxkdGU6RGVzY3JpcGNpb24+QkxPVU1FUjwvZHRlOkRlc2NyaXBjaW9uPjxkdGU6UHJlY2lvVW5pdGFyaW8+MjAuMDAwMDwvZHRlOlByZWNpb1VuaXRhcmlvPjxkdGU6UHJlY2lvPjE0MC4wMDwvZHRlOlByZWNpbz48ZHRlOkRlc2N1ZW50bz4wLjAwPC9kdGU6RGVzY3VlbnRvPjxkdGU6SW1wdWVzdG9zPjxkdGU6SW1wdWVzdG8+PGR0ZTpOb21icmVDb3J0bz5JVkE8L2R0ZTpOb21icmVDb3J0bz48ZHRlOkNvZGlnb1VuaWRhZEdyYXZhYmxlPjE8L2R0ZTpDb2RpZ29VbmlkYWRHcmF2YWJsZT48ZHRlOk1vbnRvR3JhdmFibGU+MTI1LjAwPC9kdGU6TW9udG9HcmF2YWJsZT48ZHRlOk1vbnRvSW1wdWVzdG8+MTUuMDA8L2R0ZTpNb250b0ltcHVlc3RvPjwvZHRlOkltcHVlc3RvPjwvZHRlOkltcHVlc3Rvcz48ZHRlOlRvdGFsPjE0MC4wMDwvZHRlOlRvdGFsPjwvZHRlOkl0ZW0+PC9kdGU6SXRlbXM+PGR0ZTpUb3RhbGVzPjxkdGU6VG90YWxJbXB1ZXN0b3M+PGR0ZTpUb3RhbEltcHVlc3RvIE5vbWJyZUNvcnRvPSJJVkEiIFRvdGFsTW9udG9JbXB1ZXN0bz0iMjEzLjIxIiAvPjwvZHRlOlRvdGFsSW1wdWVzdG9zPjxkdGU6R3JhblRvdGFsPjE5OTAuMDA8L2R0ZTpHcmFuVG90YWw+PC9kdGU6VG90YWxlcz48L2R0ZTpEYXRvc0VtaXNpb24+PC9kdGU6RFRFPjxkdGU6QWRlbmRhPjxub19hZGVuZGE+VGVzdCBBZGVuZGE8L25vX2FkZW5kYT48L2R0ZTpBZGVuZGE+PC9kdGU6U0FUPjwvZHRlOkdURG9jdW1lbnRvPg==";

                    var ObjFirmar = new Firmar()
                    {
                        llave = EmpresaDatosFEL.fel_token.Trim(),
                        codigo = oInfoRequerida.Referencia.Trim(),
                        //archivo = XMLB64,
                        archivo = XMLB64,
                        alias = EmpresaDatosFEL.fel_usuario.Trim(),
                        es_anulacion = "N",
                    };


                    Firmar oFirmar = new Firmar();
                    oFirmar.FirmarDocumento(ObjFirmar, EmpresaDatosFEL, Argumentos, oDatosGenerales.Tipo);
                   // Console.WriteLine("Listos para Certificar");
                    

                    try
                    {
                        Certificar ObjCertificar = new Certificar()
                        {
                            nit_emisor = oEmisorDTE.Nit,
                            correo_copia = oEmisorDTE.Correo,
                            xml_dte = oFirmar.ObtenerXMLBase64Firmado()
                        };

                        Certificar oCertificar = new Certificar();
                        oCertificar.CertificarDocumento(ObjCertificar, EmpresaDatosFEL, Argumentos, oInfoRequerida.Referencia, oDatosGenerales.Tipo);
                        
                        Environment.Exit(0);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }



                // oFirmar.FirmarDocumento(ObjFirmar, oCertificador, oArgumentos, oGenerales);

            }
            catch (Exception e)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Se ha producido un error Inesperado, posiblemente al validar el path para guardar el XML o bien al intentar firmar el documento.";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + oDatosGenerales.Tipo + "','" + Estado + "','" + Mensaje + "');";

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
        #endregion 


    }
}
