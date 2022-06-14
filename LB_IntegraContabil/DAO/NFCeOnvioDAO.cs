using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class NFCeOnvioDAO
    {
        public static async Task<IEnumerable<NFCeOnvio>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.XML_NFCe, a.TP_Ambiente, a.VerAplic,")
                    .AppendLine("a.Chave_Acesso, a.DT_Processamento, a.NR_Protocolo,")
                    .AppendLine("a.DigVal, a.Status, a.DS_Mensagem, a.CD_Empresa, a.ID_NFCe,")
                    .AppendLine("a.Id, a.Code, a.Message, a.ST_Integracao")
                    .AppendLine("from VTB_FAT_NFCEONVIO a")
                    .AppendLine("where convert(datetime, floor(convert(decimal(30,10), a.dt_emissao))) >= ")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))")
                    .AppendLine("and isnull(a.st_integracao, '') <> '1'");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<NFCeOnvio>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
    }
}
