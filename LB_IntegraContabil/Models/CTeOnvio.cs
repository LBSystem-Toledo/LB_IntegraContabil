using System;

namespace LB_IntegraContabil.Models
{
    public class CTeOnvio
    {
        public string Cd_empresa { get; set; }
        public decimal Nr_lanctoctr { get; set; }
        public string Xml_cte { get; set; }
        public decimal Nr_protocolo { get; set; }
        public string Veraplic { get; set; }
        public string Chaveacesso { get; set; }
        public DateTime Dt_processamento { get; set; }
        public string Digval { get; set; }
        public decimal Status_cte { get; set; }
        public string Msg_status { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string St_integracao { get; set; } = string.Empty;
    }
}
