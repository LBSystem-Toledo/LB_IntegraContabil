using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class BaixaOnvioDAO
    {
        public static async Task<IEnumerable<BaixaOnvio>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.Nr_Lancto, a.CD_Parcela,")
                    .AppendLine("a.ID_Liquid, a.TP_Docto, a.TP_MOV, a.Nr_Serie,")
                    .AppendLine("a.Nr_NotaFiscal, a.DT_Vencto, a.DT_Liquidacao,")
                    .AppendLine("a.Vl_Liquidacao, a.Vl_JuroAcrescimo, a.Vl_DescontoBonus,")
                    .AppendLine("a.DS_Historico, a.Nr_doctoempresa,")
                    .AppendLine("case when c.TP_Pessoa = 'F' then c.NR_CPF else c.NR_CGC end as Nr_doctoclifor,")
                    .AppendLine("a.Id, a.Code, a.Message, a.ST_Integracao")
                    .AppendLine("from VTB_FIN_BAIXASONVIO a")
                    .AppendLine("inner join VTB_FIN_CLIFOR c")
                    .AppendLine("on a.CD_Clifor = c.CD_Clifor")
                    .AppendLine("where convert(datetime, floor(convert(decimal(30,10), a.DT_Liquidacao))) >= ")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))")
                    .AppendLine("and isnull(a.st_integracao, '') <> '1'");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<BaixaOnvio>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
    }
}
