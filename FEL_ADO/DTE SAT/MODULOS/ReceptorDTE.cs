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
    public class ReceptorDTE
    {
        public string? IDReceptor { get; set; }
        public string? NombreReceptor { get; set; }
        public string? Direccion { get; set; }
        public int CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Departamento { get; set; }
        public string? Pais { get; set; }
        public string? Correo { get; set; }
        public string? drType { get; set; }

        public ReceptorDTE()
        {

        }

        public void RECEPTORXML(ReceptorDTE oReceptorDTE, XmlNode NDatosEmision, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            try
            {
                // ****----- NODO RECEPTOR
                XmlNode NReceptor = XML.CreateElement("dte", "Receptor", dte);
                NDatosEmision.AppendChild(NReceptor);

                if (oReceptorDTE.Correo.Trim() != string.Empty)
                {
                    XmlAttribute ARcorreo = XML.CreateAttribute("CorreoReceptor");
                    ARcorreo.Value = oReceptorDTE.Correo.Trim();
                    NReceptor.Attributes.Append(ARcorreo);
                }

                XmlAttribute AIDReceptor = XML.CreateAttribute("IDReceptor");
                AIDReceptor.Value = oReceptorDTE.IDReceptor;
                NReceptor.Attributes.Append(AIDReceptor);

                XmlAttribute ANombreReceptor = XML.CreateAttribute("NombreReceptor");
                ANombreReceptor.Value = oReceptorDTE.NombreReceptor;
                NReceptor.Attributes.Append(ANombreReceptor);

                if (oReceptorDTE.drType != null & oReceptorDTE.drType != string.Empty)
                {
                    XmlAttribute ATipoEspecial = XML.CreateAttribute("TipoEspecial");
                    ATipoEspecial.Value = oReceptorDTE.drType;
                    NReceptor.Attributes.Append(ATipoEspecial);
                }

                //----*****
                //*****------
                XmlNode NDireccionReceptor = XML.CreateElement("dte", "DireccionReceptor", dte);
                NReceptor.AppendChild(NDireccionReceptor);
                //----*****
                //*****------
                XmlNode NDireccionR = XML.CreateElement("dte", "Direccion", dte);
                NDireccionReceptor.AppendChild(NDireccionR);
                NDireccionR.InnerText = oReceptorDTE.Direccion;

                XmlNode NCodigoPostalR = XML.CreateElement("dte", "CodigoPostal", dte);
                NDireccionReceptor.AppendChild(NCodigoPostalR);
                NCodigoPostalR.InnerText = Convert.ToString(oReceptorDTE.CodigoPostal);

                XmlNode NMunicipioR = XML.CreateElement("dte", "Municipio", dte);
                NDireccionReceptor.AppendChild(NMunicipioR);
                NMunicipioR.InnerText = oReceptorDTE.Municipio;

                XmlNode NDepartamentoR = XML.CreateElement("dte", "Departamento", dte);
                NDireccionReceptor.AppendChild(NDepartamentoR);
                NDepartamentoR.InnerText = oReceptorDTE.Departamento;

                XmlNode NPaisR = XML.CreateElement("dte", "Pais", dte);
                NDireccionReceptor.AppendChild(NPaisR);
                NPaisR.InnerText = oReceptorDTE.Pais;
                //----*****


            }
            catch (Exception)
            {

                string StringConexionfel = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionfel))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Ocurrio un error al generar el XML, Especificamente en los Datos del Receptor";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" + TipoFactura + "','" + Estado + "','" + Mensaje + "');";

                    SqlCommand cmd = new SqlCommand(QueryBitacora, ConexionBitacora);
                    cmd.ExecuteNonQuery();
                    ConexionBitacora.Dispose();
                   
                }
                using (SqlConnection ActualizarFEL = new SqlConnection(StringConexionfel))
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
