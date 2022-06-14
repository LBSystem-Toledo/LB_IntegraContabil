using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class CancelamentoNFCeSiegDAO
    {
        public static async Task<IEnumerable<CancelamentoNFCeSieg>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.Id_Cupom, a.ID_Evento,")
                    .AppendLine("c.CD_Versao, a.XML_Evento, a.XML_RetEvento")
                    .AppendLine("from TB_PDV_EventoNFCe a")
                    .AppendLine("inner join TB_FAT_Evento b")
                    .AppendLine("on a.CD_Evento = b.CD_Evento")
                    .AppendLine("inner join TB_FAT_CFGNfe c")
                    .AppendLine("on a.CD_Empresa = c.CD_Empresa")
                    .AppendLine("where b.TP_Evento = 'CA'")
                    .AppendLine("and ISNULL(a.ST_Registro, 'A') = 'P'")
                    .AppendLine("and ISNULL(a.IntegradoSieg, 0) = 0")
                    .AppendLine("and convert(datetime, floor(convert(decimal(30,10), a.dt_evento))) >=")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -30, getdate()))))");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<CancelamentoNFCeSieg>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(CancelamentoNFCeSieg nfc, string servidor, string banco)
        {
            try
            {
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@INTEGRADOSIEG", true);
                        param.Add("@CD_EMPRESA", nfc.Cd_empresa);
                        param.Add("@ID_CUPOM", nfc.Id_cupom);
                        param.Add("@ID_EVENTO", nfc.Id_evento);
                        await conexao._conexao.ExecuteAsync("update TB_PDV_EventoNFCe set IntegradoSieg = @INTEGRADOSIEG, DT_Alt = GETDATE() where CD_Empresa = @CD_EMPRESA and Id_Cupom = @ID_CUPOM and ID_Evento = @ID_EVENTO", param, commandType: CommandType.Text);
                    }
                }
            }
            catch { }
        }
    }
}
