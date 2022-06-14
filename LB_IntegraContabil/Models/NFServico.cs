using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.Models
{
    public class NFServico
    {
        public decimal Nr_notafiscal { get; set; }
        public decimal Nr_lanctofiscal { get; set; }
        public DateTime Dt_emissao { get; set; }
        public decimal Vl_basecalcISS { get; set; }
        public decimal Pc_aliquotaISS { get; set; }
        public decimal Vl_ISS { get; set; }
        public decimal Vl_liquido { get; set; }
        public string Cnpj_empresa { get; set; }
        public string insc_municipal { get; set; }
        public string Nm_empresa { get; set; }
        public string Nm_fantasia { get; set; }
        public string Endereco_empresa { get; set; }
        public string Numero_empresa { get; set; }
        public string Bairro_empresa { get; set; }
        public string Cd_cidade_empresa { get; set; }
        public string Cep_empresa { get; set; }
        public string Tp_regimetributario { get; set; }
        public string Cd_municipioexecservico { get; set; }
        public string Uf_execservico { get; set; }
        public decimal Vl_totalProdutosServicos { get; set; }
        public decimal Vl_retidoPIS { get; set; }
        public decimal Vl_retidoCofins { get; set; }
        public decimal Vl_retidoINSS { get; set; }
        public decimal Vl_retidoIRRF { get; set; }
        public decimal Vl_retidoCSLL { get; set; }
        public decimal Vl_ISS_Retido { get; set; }
        public string Id_tpservico { get; set; }
        public string Cnae_fiscal { get; set; }
        public string Ds_produto { get; set; }
        public string Nr_doc_cliente { get; set; }
        public string Nm_cliente { get; set; }
        public string Endereco_cliente { get; set; }
        public string Numero_cliente { get; set; }
        public string Bairro_cliente { get; set; }
        public string Cd_cidade_cliente { get; set; }
        public string Uf_cliente { get; set; }
        public string Cd_pais_cliente { get; set; }
        public string Cep_cliente { get; set; }

    }
}
