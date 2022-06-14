using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class IntegraDAO
    {
        public static async Task<IEnumerable<IntegraONVIO>> GetAsync(string servidor,
                                                                     string banco,
                                                                     string Cd_empresa = "",
                                                                     string Nr_lanctofiscal = "",
                                                                     string Id_nfce = "",
                                                                     string Nr_lanctoctr = "",
                                                                     string Nr_lancto = "",
                                                                     string Cd_parcela = "",
                                                                     string Id_liquid = "")
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.cd_empresa, a.id_integra, a.nr_lanctofiscal,")
                    .AppendLine("a.id_nfce, a.nr_lancto, a.cd_parcela, a.id_liquid, a.nr_lanctoctr,")
                    .AppendLine("a.id, a.code, a.message, a.st_registro")
                    .AppendLine("FROM TB_FAT_IntegraONVIO a");
                string cond = "where";
                if (!string.IsNullOrWhiteSpace(Cd_empresa))
                {
                    sql.AppendLine(cond + " a.cd_empresa = '" + Cd_empresa.Trim() + "'");
                    cond = "and";
                }
                if(!string.IsNullOrWhiteSpace(Nr_lanctofiscal))
                {
                    sql.AppendLine(cond + " a.nr_lanctofiscal = " + Nr_lanctofiscal);
                    cond = "and";
                }
                if(!string.IsNullOrWhiteSpace(Id_nfce))
                {
                    sql.AppendLine(cond + " a.id_nfce = " + Id_nfce);
                    cond = "and";
                }
                if(!string.IsNullOrWhiteSpace(Nr_lanctoctr))
                {
                    sql.AppendLine(cond + " a.nr_lanctoctr = " + Nr_lanctoctr);
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Nr_lancto))
                {
                    sql.AppendLine(cond + " a.nr_lancto = " + Nr_lancto);
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Cd_parcela))
                {
                    sql.AppendLine(cond + " a.cd_parcela = " + Cd_parcela);
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Id_liquid))
                    sql.AppendLine(cond + " a.id_liquid = " + Id_liquid);
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<IntegraONVIO>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(IntegraONVIO val, string servidor, string banco)
        {
            try
            {
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if(await conexao.OpenConnectionAsync())
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@P_ID_INTEGRA", val.Id_integra);
                        param.Add("@P_CD_EMPRESA", val.Cd_empresa);
                        param.Add("@P_NR_LANCTOFISCAL", val.Nr_lanctofiscal);
                        param.Add("@P_ID_NFCE", val.Id_nfce);
                        param.Add("@P_NR_LANCTO", val.Nr_lancto);
                        param.Add("@P_CD_PARCELA", val.Cd_parcela);
                        param.Add("@P_ID_LIQUID", val.Id_liquid);
                        param.Add("@P_NR_LANCTOCTR", val.Nr_lanctoCTR);
                        param.Add("@P_ID", val.Id);
                        param.Add("@P_CODE", val.Code);
                        param.Add("@P_MESSAGE", val.Message);
                        param.Add("@P_ST_REGISTRO", val.St_registro);
                        await conexao._conexao.ExecuteAsync("IA_FAT_INTEGRAONVIO", param, commandType: CommandType.StoredProcedure);
                    }
                }
            }
            catch { }
        }
        public static async Task ExcluirAsync(IntegraONVIO val, string servidor, string banco)
        {
            try
            {
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@P_ID_INTEGRA", val.Id_integra);
                        await conexao._conexao.ExecuteAsync("EXCLUI_FAT_INTEGRAONVIO", param, commandType: CommandType.StoredProcedure);
                    }
                }
            }
            catch { }
        }
    }
}
