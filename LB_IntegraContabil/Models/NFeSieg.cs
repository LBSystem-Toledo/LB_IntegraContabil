using System;

namespace LB_IntegraContabil.Models
{
    public class NFeSieg
    {
        public string Cd_empresa { get; set; }
        public decimal Nr_lanctofiscal { get; set; }
        public string Xml_nfe { get; set; }
        public string Chave_acesso_nfe { get; set; }
        public decimal Nr_protocolo { get; set; }
        public string Veraplic { get; set; }
        public DateTime Dt_processamento { get; set; }
        public decimal Digitoverificado { get; set; }
        public decimal Status { get; set; }
        public string Ds_mensagem { get; set; }
        public string Tp_ambiente { get; set; }
    }
}
