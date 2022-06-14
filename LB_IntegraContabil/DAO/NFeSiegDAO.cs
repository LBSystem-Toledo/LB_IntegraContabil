using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class NFeSiegDAO
    {
        public static async Task<IEnumerable<NFeSieg>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.Nr_LanctoFiscal, a.xml_nfe,")
                    .AppendLine("a.Chave_Acesso_NFE, a.Nr_protocolo, c.VerAplic, c.DT_Processamento,")
                    .AppendLine("c.DigitoVerificado, c.Status, c.DS_Mensagem, d.Tp_Ambiente")
                    .AppendLine("from VTB_FAT_NOTAFISCAL a")
                    .AppendLine("inner join TB_FAT_SerieNF b")
                    .AppendLine("on a.CD_Modelo = b.CD_Modelo")
                    .AppendLine("and a.Nr_Serie = b.Nr_Serie")
                    .AppendLine("inner join TB_FAT_LoteNFE_X_NotaFiscal c")
                    .AppendLine("on a.CD_Empresa = c.CD_Empresa")
                    .AppendLine("and a.Nr_LanctoFiscal = c.Nr_LanctoFiscal")
                    .AppendLine("inner join TB_FAT_LoteNFE d")
                    .AppendLine("on d.ID_Lote = c.ID_Lote")
                    .AppendLine("where ISNULL(a.Nr_protocolo, '') <> ''")
                    .AppendLine("and ISNULL(b.Tp_Serie, 'P') = 'P'")
                    .AppendLine("and ISNULL(a.IntegradoSieg, 0) = 0")
                    .AppendLine("and convert(datetime, floor(convert(decimal(30,10), a.dt_emissao))) >=")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<NFeSieg>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task GravarAsync(NFeSieg nf, string servidor, string banco)
        {
            try
            {
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@INTEGRADOSIEG", true);
                        param.Add("@CD_EMPRESA", nf.Cd_empresa);
                        param.Add("@NR_LANCTOFISCAL", nf.Nr_lanctofiscal);
                        await conexao._conexao.ExecuteAsync("update tb_fat_notafiscal set IntegradoSieg = @INTEGRADOSIEG, dt_alt = getdate() " +
                                                            "where cd_empresa = @CD_EMPRESA and nr_lanctofiscal = @NR_LANCTOFISCAL", param, commandType: CommandType.Text);
                    }
                }
            }
            catch { }
        }
    }
}
