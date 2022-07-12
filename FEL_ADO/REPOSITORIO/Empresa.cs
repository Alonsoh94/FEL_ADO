using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEL_ADO.REPOSITORIO
{
    public class Empresa
    {
        public string Fel_certificador { get; set; }
        public string fel_usuario { get; set; }
        public string fel_clave { get; set; }
        public string fel_key { get; set; }
        public string fel_token { get; set; }
        public string fel_vencimiento_token { get; set; }
        public string fel_hora_vencimiento_token { get; set; }
        public string fel_url_firma { get; set; }
        public string fel_url_certifica { get; set; }
        public string fel_url_anula { get; set; }
        public string Path_XML { get; set; }
        public string fel_url_Token { get; set; }

        public Empresa()
        {

        }
    }
}
