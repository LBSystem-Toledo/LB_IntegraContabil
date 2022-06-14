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
    public static class EmpresaDAO
    {
        public static async Task<IEnumerable<Empresa>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select CD_Empresa, Chave_Cliente, Token,")
                    .AppendLine("DT_Token, Expira_em, Integration_key")
                    .AppendLine("from TB_DIV_Empresa")
                    .AppendLine("where isnull(st_registro, 'A') <> 'C'");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<Empresa>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(Empresa empresa, string servidor, string banco)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@CD_EMPRESA", empresa.Cd_empresa);
                param.Add("@TOKEN", empresa.Token);
                param.Add("@DT_TOKEN", empresa.Dt_token);
                param.Add("@EXPIRA_EM", empresa.Expira_em);
                param.Add("@INTEGRATION_KEY", empresa.Integration_key);
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        await conexao._conexao.ExecuteAsync("update tb_div_empresa set token = @TOKEN, dt_token = @DT_TOKEN, expira_em = @EXPIRA_EM, " +
                                                            "integration_key = @INTEGRATION_KEY, dt_alt = getdate() " +
                                                            "where CD_EMPRESA = @CD_EMPRESA", param, commandType: CommandType.Text);
                }
            }
            catch { }
        }
    }
}
