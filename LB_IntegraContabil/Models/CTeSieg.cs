using System;

namespace LB_IntegraContabil.Models
{
    public class CTeSieg
    {
        public string Cd_empresa { get; set; }
        public decimal Nr_lanctoctr { get; set; }
        public string Xml_cte { get; set; }
        public decimal Nr_protocolo { get; set; }
        public string ChaveAcesso { get; set; }
        public string VerAplic { get; set; }
        public DateTime Dt_processamento { get; set; }
        public string DigVal { get; set; }
        public decimal Status_cte { get; set; }
        public string Msg_stauts { get; set; }
    }
}
