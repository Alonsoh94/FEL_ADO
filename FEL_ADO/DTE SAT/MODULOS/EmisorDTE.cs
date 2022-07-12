using FEL_ADO.REPOSITORIO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ADO.DTE_SAT.MODULOS
{
    public class EmisorDTE
    {
        public string? AfiliacionIva { get; set; }
        public int Establecimiento { get; set; }
        public string? Correo { get; set; }
        public string? Nit { get; set; }
        public string? NombreComercial { get; set; }
        public string? NombreEmisor { get; set; }
        public string? Direccion { get; set; }
        public int CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Departamento { get; set; }
        public string? Pais { get; set; }

        public EmisorDTE()
        {

        }

        public void DATAOSEMISORXML(EmisorDTE oEmisorDTE, XmlNode NDatosEmision, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            try
            {
                //EmisorDTE oEmisorDTE;

                XmlNode NEmisor = XML.CreateElement("dte", "Emisor", dte);
                NDatosEmision.AppendChild(NEmisor);

                XmlAttribute AAfiliacionIVA = XML.CreateAttribute("AfiliacionIVA");
                AAfiliacionIVA.Value = oEmisorDTE.AfiliacionIva;
                NEmisor.Attributes.Append(AAfiliacionIVA);

                XmlAttribute ACodigoEstablecimiento = XML.CreateAttribute("CodigoEstablecimiento");
                ACodigoEstablecimiento.Value = Convert.ToString(oEmisorDTE.Establecimiento);
                NEmisor.Attributes.Append(ACodigoEstablecimiento);

                try
                {
                    if (oEmisorDTE.Correo == null || oEmisorDTE.Correo == string.Empty)
                    {


                    }
                    else
                    {
                        XmlAttribute ACorreoEmisor = XML.CreateAttribute("CorreoEmisor");
                        ACorreoEmisor.Value = oEmisorDTE.Correo;
                        NEmisor.Attributes.Append(ACorreoEmisor);
                    }

                }
                catch (Exception)
                {


                }


                XmlAttribute ANITEmisor = XML.CreateAttribute("NITEmisor");
                ANITEmisor.Value = oEmisorDTE.Nit;
                NEmisor.Attributes.Append(ANITEmisor);

                XmlAttribute ANombreComercial = XML.CreateAttribute("NombreComercial");
                ANombreComercial.Value = oEmisorDTE.NombreComercial;
                NEmisor.Attributes.Append(ANombreComercial);

                XmlAttribute ANombreEmisor = XML.CreateAttribute("NombreEmisor");
                ANombreEmisor.Value = oEmisorDTE.NombreEmisor;
                NEmisor.Attributes.Append(ANombreEmisor);
                //----*****

                //*****------
                XmlNode NDireccionEmisor = XML.CreateElement("dte", "DireccionEmisor", dte);
                NEmisor.AppendChild(NDireccionEmisor);    //----*****
                                                          //*****------
                XmlNode NDireccion = XML.CreateElement("dte", "Direccion", dte);
                NDireccionEmisor.AppendChild(NDireccion);
                NDireccion.InnerText = oEmisorDTE.Direccion;

                XmlNode NCodigoPostal = XML.CreateElement("dte", "CodigoPostal", dte);
                NDireccionEmisor.AppendChild(NCodigoPostal);
                NCodigoPostal.InnerText = Convert.ToString(oEmisorDTE.CodigoPostal);

                XmlNode NMunicipio = XML.CreateElement("dte", "Municipio", dte);
                NDireccionEmisor.AppendChild(NMunicipio);
                NMunicipio.InnerText = oEmisorDTE.Municipio;

                XmlNode NDepartamento = XML.CreateElement("dte", "Departamento", dte);
                NDireccionEmisor.AppendChild(NDepartamento);
                NDepartamento.InnerText = oEmisorDTE.Departamento;

                XmlNode NPais = XML.CreateElement("dte", "Pais", dte);
                NDireccionEmisor.AppendChild(NPais);
                NPais.InnerText = oEmisorDTE.Pais;    //----*****

            }
            catch (Exception)
            {

                string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Ocurrio un error al generar el XML, Especificamente en los Datos del Emisor";
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
