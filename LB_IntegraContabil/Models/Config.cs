namespace LB_IntegraContabil.Models
{
    public class Config
    {
        public int Id_cfg { get; set; }
        public string Url_dominio { get; set; } = string.Empty;
        public string Url_oauth { get; set; } = string.Empty;
        public string Client_id { get; set; } = string.Empty;
        public string Client_secret { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Url_sieg { get; set; } = string.Empty;
        public string Email_sieg { get; set; } = string.Empty;
        public string Key_sieg { get; set; } = string.Empty;
    }
}
