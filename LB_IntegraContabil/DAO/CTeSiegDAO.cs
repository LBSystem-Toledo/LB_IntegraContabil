using Dapper;
using LB_IntegraContabil.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class CTeSiegDAO
    {
        public static async Task<IEnumerable<CTeSieg>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.NR_LanctoCTR, a.xml_cte,")
                    .AppendLine("a.NR_Protocolo, a.ChaveAcesso, a.VerAplic,")
                    .AppendLine("a.DT_Processamento, a.DigVal, a.status_cte, a.msg_status")
                    .AppendLine("from VTB_CTR_CONHECIMENTOFRETE a")
                    .AppendLine("where a.NR_Protocolo is not null")
                    .AppendLine("and ISNULL(a.IntegradoSieg, 0) = 0")
                    .AppendLine("and a.Cd_Modelo = '57'")
                    .AppendLine("and convert(datetime, floor(convert(decimal(30,10), a.DT_Processamento))) >=")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<CTeSieg>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(CTeSieg cte, string servidor, string banco)
        {
            try
            {
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@INTEGRADOSIEG", true);
                        param.Add("@CD_EMPRESA", cte.Cd_empresa);
                        param.Add("@NR_LANCTOCTR", cte.Nr_lanctoctr);
                        await conexao._conexao.ExecuteAsync("update TB_CTR_ConhecimentoFrete set IntegradoSieg = @INTEGRADOSIEG, DT_Alt = GETDATE() " +
                                                            "where CD_Empresa = @CD_EMPRESA and NR_LanctoCTR = @NR_LANCTOCTR", param, commandType: CommandType.Text);
                    }
                }
            }
            catch { }
        }
    }
}
