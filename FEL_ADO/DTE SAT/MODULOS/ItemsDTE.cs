using FEL_ADO.REPOSITORIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ADO.DTE_SAT.MODULOS
{
    public class ItemsDTE
    {
        public int Id { get; set; }
        public int NumeroLinea { get; set; }
        public string? BienOservicio { get; set; }
        public int Cantidad { get; set; }

        public string? UnidadMedida { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Precio { get; set; }
        public decimal Descuento { get; set; }

        public ItemsDTE()
        {

        }

        public void ITEMSXML(XmlNode NDatosEmision, dynamic XML, string dte, ListaArgumentos Argumentos, string TipoFactura)
        {
            int Id_Doc = Convert.ToInt32(Argumentos.Id_Documento);
            string StringConexionFEL = string.Format("Server={0};Database={1};User ID={2};Password={3}", Argumentos.Servidor, Argumentos.DataBaseFEL, Argumentos.Usuario, Argumentos.clave);

            try
            {
                string QueryFrases = "select * From feel_items where dte_id = " + Id_Doc;
                using (SqlConnection ConexionItems = new SqlConnection(StringConexionFEL))
                {
                    ConexionItems.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter(QueryFrases, ConexionItems);
                    DataSet ds = new DataSet();
                    adaptador.Fill(ds);
                    DataTable TablaDatosItems = ds.Tables[0];

                    if (TablaDatosItems.Rows.Count != 0)
                    {
                        string TotalItem = string.Empty;
                        XmlNode NItems = XML.CreateElement("dte", "Items", dte);  // nodo Items
                        NDatosEmision.AppendChild(NItems);

                        foreach (DataRow MIItem in TablaDatosItems.Rows)
                        {
                            int IdItem = Convert.ToInt32(MIItem[0]);
                            XmlNode NItem = XML.CreateElement("dte", "Item", dte);  // nodo Item
                            NItems.AppendChild(NItem);

                            XmlAttribute ANumeroLinea = XML.CreateAttribute("NumeroLinea");
                            ANumeroLinea.Value = Convert.ToString(MIItem[2]);
                            NItem.Attributes.Append(ANumeroLinea);

                            XmlAttribute ABienOServicio = XML.CreateAttribute("BienOServicio");
                            ABienOServicio.Value = Convert.ToString(MIItem[3]);
                            NItem.Attributes.Append(ABienOServicio);

                            XmlNode NCantidad = XML.CreateElement("dte", "Cantidad", dte);
                            NItem.AppendChild(NCantidad);
                            NCantidad.InnerText = Convert.ToString(MIItem[4]);

                            if (Convert.ToString(MIItem[5]) != string.Empty)
                            {
                                XmlNode NUnidadMedida = XML.CreateElement("dte", "UnidadMedida", dte);
                                NItem.AppendChild(NUnidadMedida);
                                NUnidadMedida.InnerText = Convert.ToString(MIItem[5]);

                            }

                            XmlNode NDescripcion = XML.CreateElement("dte", "Descripcion", dte);
                            NItem.AppendChild(NDescripcion);
                            NDescripcion.InnerText = Convert.ToString(MIItem[6]);

                            XmlNode NPrecioUnitario = XML.CreateElement("dte", "PrecioUnitario", dte);
                            NItem.AppendChild(NPrecioUnitario);
                            NPrecioUnitario.InnerText = Convert.ToString(MIItem[7]);

                            XmlNode NPrecio = XML.CreateElement("dte", "Precio", dte);
                            NItem.AppendChild(NPrecio);
                            NPrecio.InnerText = Convert.ToString(MIItem[8]);
                            decimal Precio = Convert.ToDecimal(MIItem[8]);

                            XmlNode NDescuento = XML.CreateElement("dte", "Descuento", dte);
                            NItem.AppendChild(NDescuento);
                            NDescuento.InnerText = Convert.ToString(MIItem[9]);
                            decimal Descuento = Convert.ToDecimal(MIItem[9]);

                            decimal TotalItemCalculado = Precio - Descuento;

                            // SECCION DE ITEMS IMPUESTOS
                            if (TipoFactura != "NABN")
                            {
                                ImpuestosItemDTE ItemImpuesto = new ImpuestosItemDTE();
                                ItemImpuesto.IMPUESTOSITEMSXML(NItem, XML, dte, IdItem, Argumentos, TipoFactura);
                            }

                            XmlNode NTotal = XML.CreateElement("dte", "Total", dte);
                            NItem.AppendChild(NTotal);
                            NTotal.InnerText = Convert.ToString(TotalItemCalculado);

                        }

                    }
                    else
                    {
                        using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                        {
                            ConexionBitacora.Open();
                            string Estado = "ERROR";
                            string Mensaje = "No se Encontraron datos en Items";
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

                    
                    ConexionItems.Close();
                }
            }

            catch (Exception)
            {

                using (SqlConnection ConexionBitacora = new SqlConnection(StringConexionFEL))
                {
                    ConexionBitacora.Open();
                    string Estado = "ERROR";
                    string Mensaje = "No se Encontraron datos en Impuestos Items";
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
