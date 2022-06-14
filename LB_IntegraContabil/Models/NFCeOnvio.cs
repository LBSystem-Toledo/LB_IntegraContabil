using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.Models
{
    public class NFCeOnvio
    {
        public string Cd_empresa { get; set; }
        public decimal Id_nfce { get; set; }
        public string Tp_ambiente { get; set; }
        public string VerAplic { get; set; }
        public string Chave_acesso { get; set; }
        public DateTime Dt_processamento { get; set; }
        public decimal Nr_protocolo { get; set; }
        public string Xml_nfce { get; set; }
        public string DigVal { get; set; }
        public decimal Status { get; set; }
        public string Ds_mensagem { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string St_integracao { get; set; } = string.Empty;
    }
}
