using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class CTeOnvioDAO
    {
        public static async Task<IEnumerable<CTeOnvio>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.NR_LanctoCTR, a.xml_cte,")
                    .AppendLine("a.NR_Protocolo, a.VerAplic, a.ChaveAcesso,")
                    .AppendLine("a.DT_Processamento, a.DigVal, a.status_cte, a.msg_status,")
                    .AppendLine("a.Id, a.Code, a.Message, a.ST_Integracao")
                    .AppendLine("from VTB_FAT_CTEONVIO a")
                    .AppendLine("where convert(datetime, floor(convert(decimal(30,10), a.dt_emissao))) >= ")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))")
                    .AppendLine("and isnull(a.st_integracao, '') <> '1'");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<CTeOnvio>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
    }
}
