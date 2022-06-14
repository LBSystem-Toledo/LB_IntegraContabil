using System;

namespace LB_IntegraContabil.Models
{
    public class Empresa
    {
        public string Cd_empresa { get; set; } = string.Empty;
        public string Chave_cliente { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime? Dt_token { get; set; } = null;
        public int Expira_em { get; set; } = 0;
        public DateTime? Dt_expira
        {
            get
            {
                if (Dt_token.HasValue)
                    return Dt_token.Value.AddSeconds(Expira_em);
                else return null;
            }
        }
        public string Integration_key { get; set; } = string.Empty;
    }
}
