using System;

namespace LB_IntegraContabil.Models
{
    public class LoteNFe
    {
        public string Tp_ambiente { get; set; }
        public string Veraplic { get; set; }
        public DateTime Dt_processamento { get; set; }
        public decimal Nr_protocolo { get; set; }
        public string Digitoverificado { get; set; }
        public decimal Status { get; set; }
        public string Ds_mensagem { get; set; }
    }
}
