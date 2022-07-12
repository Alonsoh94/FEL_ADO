using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEL_ADO.REPOSITORIO
{
    public class ListaArgumentos
    {
        public string Servidor { get; set; } = String.Empty;
        public string DataBaseEmpresa { get; set; } = String.Empty;
        public string DataBaseFEL { get; set; } = String.Empty;
        public string Id_Empresa { get; set; } = String.Empty;
        public string Id_Documento { get; set; } = String.Empty;
        public string Tipo_Transaccion { get; set; } = String.Empty;
        public string Usuario { get; set; } = String.Empty;
        public string clave { get; set; } = String.Empty;
        public string Establecimiento { get; set; } = String.Empty;

        public ListaArgumentos()
        {

        }
    }
}
