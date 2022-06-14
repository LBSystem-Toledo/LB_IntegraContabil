using System;

namespace LB_IntegraContabil.Models
{
    public class NFCeSieg
    {
        public string Cd_empresa { get; set; }
        public decimal id_nfce { get; set; }
        public string Xml_nfce { get; set; }
        public string Chave_acesso { get; set; }
        public string Tp_ambiente { get; set; }
        public string Veraplic { get; set; }
        public DateTime Dt_processamento { get; set; }
        public decimal Nr_protocolo { get; set; }
        public string Digval { get; set; }
        public decimal Status { get; set; }
        public string Ds_mensagem { get; set; }
    }
}
