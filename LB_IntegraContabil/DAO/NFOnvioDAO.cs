using Dapper;
using LB_IntegraContabil.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public static class NFOnvioDAO
    {
        public static async Task<IEnumerable<NFOnvio>> GetAsync(string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.CD_Empresa, a.NM_Empresa, a.CD_Clifor,")
                    .AppendLine("a.NM_Clifor, a.Nr_LanctoFiscal, a.Nr_NotaFiscal,")
                    .AppendLine("a.Nr_Serie, a.NR_RPS, a.Tp_Movimento, a.Tp_Nota,")
                    .AppendLine("a.DT_Emissao, a.DT_SaiEnt, a.Chave_Acesso_NFE,")
                    .AppendLine("a.Xml_nfe, a.Vl_totalnota, a.id, a.Code, a.Message,")
                    .AppendLine("a.ST_Integracao, a.CD_Movimentacao, a.DS_Movimentacao")
                    .AppendLine("from VTB_FAT_NFONVIO a")
                    .AppendLine("where convert(datetime, floor(convert(decimal(30,10), a.dt_emissao))) >= ")
                    .AppendLine("convert(datetime, floor(convert(decimal(30,10), dateadd(day, -90, getdate()))))")
                    .AppendLine("and isnull(a.st_integracao, '') <> '1'");
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<NFOnvio>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task<NFServico> GetNFServicoAsync(string Cd_empresa, string Nr_lanctofiscal, string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.Nr_NotaFiscal, a.Nr_LanctoFiscal, a.DT_Emissao,")
                    .AppendLine("itens.Vl_basecalcISS, itens.Pc_aliquotaISS, itens.Vl_iss,")
                    .AppendLine("itens.Vl_liquido, b.NR_CGC as Cnpj_empresa, b.Insc_Municipal,")
                    .AppendLine("b.NM_Empresa, b.NM_Fantasia, b.CD_Endereco as Endereco_empresa,")
                    .AppendLine("b.Numero as Numero_empresa, b.Bairro as Bairro_empresa,")
                    .AppendLine("b.CD_Cidade as Cd_cidade_empresa, b.Cep as Cep_empresa,")
                    .AppendLine("b.TP_RegimeTributario, isnull(a.cd_municipioexecservico, b.cd_cidade) as cd_municipioexecservico,")
                    .AppendLine("isnull(d.UF, b.UF) as Uf_execservico, itens.Vl_totalProdutosServicos,")
                    .AppendLine("itens.Vl_retidoPIS, itens.Vl_retidoCofins, itens.Vl_retidoINSS,")
                    .AppendLine("itens.Vl_retidoIRRF, itens.Vl_retidoCSLL, itens.Vl_ISS_Retido,")
                    .AppendLine("itens.ID_TPServico, itens.DS_Produto, b.CNAE_Fiscal,")
                    .AppendLine("case when e.TP_Pessoa = 'F' then e.NR_CPF else e.NR_CGC end as Nr_doc_cliente,")
                    .AppendLine("e.NM_Clifor as Nm_cliente, f.DS_Endereco as Endereco_cliente,")
                    .AppendLine("f.Numero as Numero_cliente, f.Bairro as Bairro_cliente,")
                    .AppendLine("f.CD_Cidade as Cd_cidade_cliente, f.UF as Uf_cliente,")
                    .AppendLine("f.CD_PAIS as Cd_pais_cliente, f.Cep as Cep_cliente")
                    .AppendLine("from VTB_FAT_NOTAFISCAL a")
                    .AppendLine("inner join VTB_DIV_EMPRESA b")
                    .AppendLine("on a.CD_Empresa = b.CD_Empresa")
                    .AppendLine("and isnull(b.chave_cliente, '') <> ''")
                    .AppendLine("left join TB_FIN_Cidade c")
                    .AppendLine("on a.cd_municipioexecservico = c.CD_Cidade")
                    .AppendLine("left join TB_FIN_UF d")
                    .AppendLine("on c.CD_UF = d.CD_UF")
                    .AppendLine("inner join VTB_FIN_CLIFOR e")
                    .AppendLine("on a.CD_Clifor = e.CD_Clifor")
                    .AppendLine("inner join VTB_FIN_ENDERECO f")
                    .AppendLine("on a.CD_Clifor = f.CD_Clifor")
                    .AppendLine("and a.CD_Endereco = f.CD_Endereco")
                    .AppendLine("outer apply")
                    .AppendLine("(")
                    .AppendLine("    select top 1 SUM(x.VL_BASECALCISS) as Vl_basecalcISS,")
                    .AppendLine("    AVG(x.PC_ALIQUOTAISS) as Pc_aliquotaISS, SUM(x.VL_ISS) as Vl_iss,")
                    .AppendLine("    SUM(x.Vl_SubTotal - case when x.TP_TRIBUTISS = 'R' then x.VL_ISS else 0 end -")
                    .AppendLine("    x.Vl_RetidoCofins - x.Vl_RetidoCSLL - x.Vl_RetidoINSS - x.Vl_RetidoIRRF - x.Vl_RetidoPIS) as Vl_liquido,")
                    .AppendLine("    SUM(x.Vl_SubTotal) as Vl_totalProdutosServicos,")
                    .AppendLine("    SUM(x.Vl_RetidoPIS) as Vl_retidoPIS,")
                    .AppendLine("    SUM(x.Vl_RetidoCofins) as Vl_retidoCofins,")
                    .AppendLine("    SUM(x.Vl_RetidoINSS) as Vl_retidoINSS,")
                    .AppendLine("    SUM(x.Vl_RetidoIRRF) as Vl_retidoIRRF,")
                    .AppendLine("    SUM(x.Vl_RetidoCSLL) as Vl_retidoCSLL,")
                    .AppendLine("    SUM(case when x.TP_TRIBUTISS = 'R' then x.VL_ISS else 0 end) as Vl_ISS_Retido,")
                    .AppendLine("    y.ID_TPServico, y.DS_Produto")
                    .AppendLine("    from VTB_FAT_NOTAFISCAL_ITEM x")
                    .AppendLine("    inner join TB_EST_Produto y")
                    .AppendLine("    on x.CD_Produto = y.CD_Produto")
                    .AppendLine("    where x.CD_Empresa = a.CD_Empresa")
                    .AppendLine("    and x.Nr_LanctoFiscal = a.Nr_LanctoFiscal")
                    .AppendLine("    group by y.ID_TPServico, y.DS_Produto")
                    .AppendLine(") as itens")
                    .AppendLine("where a.CD_Empresa = '" + Cd_empresa.Trim() + "'")
                    .AppendLine("and a.Nr_LanctoFiscal = " + Nr_lanctofiscal);
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryFirstOrDefaultAsync<NFServico>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
        public static async Task<LoteNFe> GetLoteNFeAsync(string Cd_empresa, string Nr_lanctofiscal, string servidor, string banco)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.Tp_Ambiente, b.VerAplic, b.DT_Processamento,")
                    .AppendLine("b.Nr_Protocolo, b.DigitoVerificado, b.Status, b.DS_Mensagem")
                    .AppendLine("from TB_FAT_LoteNFE a")
                    .AppendLine("inner join TB_FAT_LoteNFE_X_NotaFiscal b")
                    .AppendLine("on a.ID_Lote = b.ID_Lote")
                    .AppendLine("where b.CD_Empresa = '" + Cd_empresa.Trim() + "'")
                    .AppendLine("and b.Nr_LanctoFiscal = " + Nr_lanctofiscal);
                using (TConexao conexao = new TConexao(servidor, banco))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryFirstOrDefaultAsync<LoteNFe>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }
    }
}
