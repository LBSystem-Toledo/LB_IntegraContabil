using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class NFCeSiegDAO
    {
        public static async Task<IEnumerable<NFCeSieg>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.ID_NFCe, d.XML_NFCe,")
                    .AppendLine("a.Chave_Acesso, c.TP_Ambiente, b.VerAplic,")
                    .AppendLine("b.DT_Processamento, b.NR_Protocolo,")
                    .AppendLine("b.DigVal, b.Status, b.DS_Mensagem")
                    .AppendLine("from VTB_PDV_NFCE a")
                    .AppendLine("inner join TB_FAT_Lote_X_NFCe b")
                    .AppendLine("on a.CD_Empresa = b.CD_Empresa")
                    .AppendLine("and a.ID_NFCe = b.Id_Cupom")
                    .AppendLine("inner join TB_FAT_LoteNFCe c")
                    .AppendLine("on b.CD_Empresa = c.CD_Empresa")
                    .AppendLine("and b.ID_Lote = c.ID_Lote")
                    .AppendLine("inner join TB_PDV_XML_NFCe d")
                    .AppendLine("on a.CD_Empresa = d.CD_Empresa")
                    .AppendLine("and a.ID_NFCe = d.ID_NFCe")
                    .AppendLine("where b.NR_Protocolo is not null")
                    .AppendLine("and ISNULL(d.IntegradoSieg, 0) = 0")
                    .AppendLine("and convert(datetime, floor(convert(decimal(30,10), b.DT_Processamento))) >=")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<NFCeSieg>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(NFCeSieg nfc, string servidor, string banco)
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
                        param.Add("@ID_NFCE", nfc.id_nfce);
                        await conexao._conexao.ExecuteAsync("update TB_PDV_XML_NFCe set IntegradoSieg = @INTEGRADOSIEG, dt_alt = getdate() " +
                                                            "where CD_Empresa = @CD_EMPRESA and ID_NFCe = @ID_NFCE", param, commandType: CommandType.Text);
                    }
                }
            }
            catch { }
        }
    }
}
