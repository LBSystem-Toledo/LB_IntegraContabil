using System;

namespace LB_IntegraContabil.Models
{
    public class NFOnvio
    {
        public string Cd_empresa { get; set; } = string.Empty;
        public string Nm_empresa { get; set; } = string.Empty;
        public string Cd_clifor { get; set; } = string.Empty;
        public string Nm_clifor { get; set; } = string.Empty;
        public decimal Nr_lanctofiscal { get; set; } = decimal.Zero;
        public decimal Nr_notafiscal { get; set; } = decimal.Zero;
        public string Nr_serie { get; set; } = string.Empty;
        public decimal? Nr_rps { get; set; } = null;
        public string Tp_movimento { get; set; } = string.Empty;
        public string Tp_nota { get; set; } = string.Empty;
        public DateTime Dt_emissao { get; set; }
        public DateTime Dt_saient { get; set; }
        public string Chave_acesso_nfe { get; set; } = string.Empty;
        public decimal Vl_totalnota { get; set; } = decimal.Zero;
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string St_integracao { get; set; } = string.Empty;
        public string Cd_movimentacao { get; set; } = string.Empty;
        public string Ds_movimentacao { get; set; } = string.Empty;
        public string Xml_nfe { get; set; } = string.Empty;
    }
}
