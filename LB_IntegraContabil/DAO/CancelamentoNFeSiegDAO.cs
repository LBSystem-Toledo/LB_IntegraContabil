using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class CancelamentoNFeSiegDAO
    {
        public static async Task<IEnumerable<CancelamentoNFeSieg>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.ID_Evento, a.XML_Evento, a.XML_RetEvento, c.CD_Versao")
                    .AppendLine("from TB_FAT_EventoNFe a")
                    .AppendLine("inner join TB_FAT_Evento b")
                    .AppendLine("on a.CD_Evento = b.CD_Evento")
                    .AppendLine("inner join TB_FAT_CFGNfe c")
                    .AppendLine("on a.CD_Empresa = c.CD_Empresa")
                    .AppendLine("where b.TP_Evento = 'CA'")
                    .AppendLine("and a.NR_Protocolo is not null")
                    .AppendLine("and ISNULL(a.IntegradoSieg, 0) = 0")
                    .AppendLine("and convert(datetime, floor(convert(decimal(30,10), a.dt_evento))) >=")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -30, getdate()))))");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<CancelamentoNFeSieg>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(CancelamentoNFeSieg nf, string servidor, string banco)
        {
            try
            {
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@INTEGRADOSIEG", true);
                        param.Add("@ID_EVENTO", nf.Id_evento);
                        await conexao._conexao.ExecuteAsync("update TB_FAT_EventoNFe set IntegradoSieg = @INTEGRADOSIEG, DT_Alt = GETDATE() where ID_Evento = @ID_EVENTO", param, commandType: CommandType.Text);
                    }
                }
            }
            catch { }
        }
    }
}
