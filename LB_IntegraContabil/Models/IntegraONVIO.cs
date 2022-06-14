using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.Models
{
    public class IntegraONVIO
    {
        public int? Id_integra { get; set; } = null;
        public string Cd_empresa { get; set; } = string.Empty;
        public decimal? Nr_lanctofiscal { get; set; } = null;
        public decimal? Id_nfce { get; set; } = null;
        public decimal? Nr_lancto { get; set; } = null;
        public decimal? Cd_parcela { get; set; } = null;
        public decimal? Id_liquid { get; set; } = null;
        public decimal? Nr_lanctoCTR { get; set; } = null;
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string St_registro { get; set; } = string.Empty;
    }
}
