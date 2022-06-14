using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.Models
{
    public class BaixaOnvio
    {
        public string Cd_empresa { get; set; }
        public decimal Nr_lancto { get; set; }
        public decimal Cd_parcela { get; set; }
        public decimal Id_liquid { get; set; }
        public string Tp_docto { get; set; }
        public string Tp_mov { get; set; }
        public string Nr_serie { get; set; }
        public decimal Nr_notafiscal { get; set; }
        public DateTime Dt_vencto { get; set; }
        public DateTime Dt_liquidacao { get; set; }
        public decimal Vl_liquidacao { get; set; }
        public decimal Vl_juroacrescimo { get; set; }
        public decimal Vl_descontobonus { get; set; }
        public string Ds_historico { get; set; }
        public string Nr_doctoempresa { get; set; }
        public string Nr_doctoclifor { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string St_integracao { get; set; } = string.Empty;
    }
}
