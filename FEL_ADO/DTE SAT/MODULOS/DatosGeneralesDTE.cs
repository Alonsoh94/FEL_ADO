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
    public class DatosGeneralesDTE
    {
        public string? CodigoMoneda { get; set; }
        public DateTime FechaHoraEmision { get; set; }
        public string? Tipo { get; set; } = String.Empty;
        public int NumeroAcceso { get; set; }
        public string? Exportacion { get; set; } = string.Empty;

        public DatosGeneralesDTE()
        {

        }

        public void DATOSGENERALESXML(DatosGeneralesDTE oDatosGenerales, XmlNode NDatosEmision, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            try
            {
                // ****-----
                XmlNode NDatosGenerales = XML.CreateElement("dte", "DatosGenerales", dte);
                NDatosEmision.AppendChild(NDatosGenerales);

                XmlAttribute CodigoMoneda = XML.CreateAttribute("CodigoMoneda");
                CodigoMoneda.Value = oDatosGenerales.CodigoMoneda;
                NDatosGenerales.Attributes.Append(CodigoMoneda);


                if (oDatosGenerales.Exportacion == "SI")
                {
                    XmlAttribute AExportacion = XML.CreateAttribute("Exp");
                    AExportacion.Value = oDatosGenerales.Exportacion;
                    NDatosGenerales.Attributes.Append(AExportacion);
                }

                //TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById("América/Guatemala");
                //est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                //DateTime FechaHora = oGenerales.FechaHoraEmision; //.ToString("yyyy-MM-ddTHH:mm:ss.fffzz:ff");
                //DateTime FechaHora = oGenerales.FechaHoraEmision.ToString("yyyy-MM-ddTHH:mm:ss.fffzz:ff");
                string FechaHora = oDatosGenerales.FechaHoraEmision.ToString("yyyy-MM-dd");
                string Zonahoraria = "T00:00:00.000-06:00";
                string FechaHOraCompleta = FechaHora + Zonahoraria;
                //TimeZoneInfo.ConvertTimeBySystemTimeZoneId(FechaHora, hwZone.ToString());
                XmlAttribute NFechaHoraEmision = XML.CreateAttribute("FechaHoraEmision");
                NFechaHoraEmision.Value = Convert.ToString(FechaHOraCompleta);
                NDatosGenerales.Attributes.Append(NFechaHoraEmision);

                XmlAttribute NTipo = XML.CreateAttribute("Tipo");
                NTipo.Value = oDatosGenerales.Tipo;
                NDatosGenerales.Attributes.Append(NTipo);
                //----*****

            }
            catch (Exception)
            {

                string StringConexionFEL= string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "Ocurrio un Error al generar el XML, especificamente en los datos Generales";
                    string QueryBitacora = "insert into feel_bitacora ( dte_id,Tipo_transaccion, Tipo_documento, Estado, Mensaje) values ('" + Argumentos.Id_Documento + "','" + Argumentos.Tipo_Transaccion + "','" +TipoFactura + "','" + Estado + "','" + Mensaje + "');";

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
