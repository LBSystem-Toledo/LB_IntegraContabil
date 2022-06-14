using Dapper;
using LB_IntegraContabil.Models;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class ConfigDAO
    {
        public static async Task<Config> GetConfigAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select Url_Dominio, Url_Oauth, Client_id,")
                    .AppendLine("Client_secret, Audience, url_sieg,")
                    .AppendLine("Id_cfg, email_sieg, key_sieg")
                    .AppendLine("from TB_DIV_CFGGeral");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryFirstOrDefaultAsync<Config>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
    }
}
