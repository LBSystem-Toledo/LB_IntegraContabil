using LB_IntegraContabil.DAO;
using LB_IntegraContabil.Models;
using LB_IntegraContabil.Service;
using LB_IntegraContabil.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LB_IntegraContabil
{
    public class IntegraContabil
    {
        
        public void Start()
        {
            Task[] tarefas = new Task[ConfigurationManager.AppSettings.AllKeys.Length];
            for (int i = 0; i < tarefas.Length; i++)
            {
                string con = ConfigurationManager.AppSettings[ConfigurationManager.AppSettings.AllKeys[i]];
                tarefas[i] = Task.Factory.StartNew(() => ExecutarTarefa(con));
            }
            while (true) { }
        }

        private async void ExecutarTarefa(string conexao)
        {
            Config config = await ConfigDAO.GetConfigAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
            IEnumerable<Empresa> empresas = await EmpresaDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
            if (config != null)
            {
                if (!string.IsNullOrWhiteSpace(config.Url_oauth) &&
                    !string.IsNullOrWhiteSpace(config.Client_id) &&
                    !string.IsNullOrWhiteSpace(config.Client_secret) &&
                    !string.IsNullOrWhiteSpace(config.Url_dominio) &&
                    !string.IsNullOrWhiteSpace(config.Audience))
                {
                    do
                    {
                        try
                        {
                            #region NF-e
                            IEnumerable<NFOnvio> nFOnvios = await NFOnvioDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var p in nFOnvios)
                            {
                                //Buscar config empresa
                                Empresa emp = empresas.First(x => x.Cd_empresa == p.Cd_empresa);
                                bool token_valido = true;
                                if (emp.Dt_expira < DateTime.Now || string.IsNullOrWhiteSpace(emp.Token) || string.IsNullOrWhiteSpace(emp.Integration_key))
                                {
                                    try
                                    {
                                        TokenONVIO token = await DataService.GerarTokenONVIOAsync(config);
                                        if (token != null)
                                        {
                                            emp.Token = token.access_token;
                                            emp.Dt_token = token.Dt_token;
                                            emp.Expira_em = int.Parse(token.expires_in);
                                            //Gerar Key integração
                                            IntegrationKey key = await DataService.GerarKeyIntegracaoAsync(emp, config.Url_dominio, token.access_token);
                                            if (key != null)
                                            {
                                                emp.Integration_key = key.integrationKey;
                                                await EmpresaDAO.GravarAsync(emp, conexao.Split(':')[0], conexao.Split(':')[1]);
                                                token_valido = true;
                                            }
                                            else token_valido = false;
                                        }
                                        else token_valido = false;
                                    }
                                    catch { token_valido = false; }
                                }
                                if (token_valido)
                                    if (string.IsNullOrWhiteSpace(p.Code))
                                    {
                                        string xml = string.Empty;
                                        //Buscar XML NF
                                        if (p.Nr_rps.HasValue)
                                        {
                                            NFServico nf = await NFOnvioDAO.GetNFServicoAsync(p.Cd_empresa, p.Nr_lanctofiscal.ToString(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            //Montar XML
                                            StringBuilder x = new StringBuilder();
                                            x.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                                            x.Append("<CompNfse xmlns=\"http://www.abrasf.org.br/nfse.xsd\">\n");
                                            x.Append("<Nfse versao=\"1.00\">\n");
                                            x.Append("<InfNfse Id=\"\">\n");
                                            x.Append("<Numero>" + nf.Nr_notafiscal.ToString() + "</Numero>\n");
                                            x.Append("<CodigoVerificacao>" + nf.Nr_lanctofiscal.ToString() + "</CodigoVerificacao>\n");
                                            x.Append("<DataEmissao>" + nf.Dt_emissao.ToString("yyyy-MM-ddTHH:mm:ss") + " </DataEmissao>\n");
                                            #region Valores
                                            x.Append("<ValoresNfse>\n");
                                            x.Append("<BaseCalculo>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_basecalcISS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</BaseCalculo>\n");
                                            x.Append("<Aliquota>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Pc_aliquotaISS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</Aliquota>");
                                            x.Append("<ValorIss>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_ISS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorIss>\n");
                                            x.Append("<ValorLiquidoNfse>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_liquido)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorLiquidoNfse>\n");
                                            x.Append("</ValoresNfse>\n");
                                            #endregion
                                            #region Prestador Servico
                                            x.Append("<PrestadorServico>\n");
                                            #region Identificação Prestador
                                            x.Append("<IdentificacaoPrestador>\n");
                                            x.Append("<CpfCnpj>\n");
                                            x.Append("<Cnpj>" + nf.Cnpj_empresa.SoNumero() + "</Cnpj>\n");
                                            x.Append("</CpfCnpj>\n");
                                            x.Append("<InscricaoMunicipal>" + nf.insc_municipal.SoNumero() + "</InscricaoMunicipal>\n");
                                            x.Append("</IdentificacaoPrestador>\n");
                                            #endregion
                                            x.Append("<RazaoSocial>" + nf.Nm_empresa.Trim() + "</RazaoSocial>\n");
                                            x.Append("<NomeFantasia>" + nf.Nm_fantasia.Trim() + "</NomeFantasia>\n");
                                            #region Endereço Prestador
                                            x.Append("<Endereco>\n");
                                            x.Append("<Endereco>" + nf.Endereco_empresa.Trim() + "</Endereco>\n");
                                            x.Append("<Numero>" + nf.Numero_empresa.Trim() + "</Numero>\n");
                                            x.Append("<Bairro>" + nf.Bairro_empresa.Trim() + "</Bairro>\n");
                                            x.Append("<CodigoMunicipio>" + nf.Cd_cidade_empresa + "</CodigoMunicipio>\n");
                                            x.Append("<CodigoPais>1058</CodigoPais>\n");
                                            x.Append("<Cep>" + nf.Cep_empresa.SoNumero() + "</Cep>\n");
                                            x.Append("</Endereco>\n");
                                            #endregion
                                            x.Append("</PrestadorServico>\n");
                                            #endregion
                                            #region Orgão Gerador
                                            x.Append("<OrgaoGerador>\n");
                                            x.Append("<CodigoMunicipio>" + nf.Cd_municipioexecservico + "</CodigoMunicipio>\n");
                                            x.Append("<UfPrestador>" + nf.Uf_execservico.Trim() + "</UfPrestador>\n");
                                            x.Append("</OrgaoGerador>\n");
                                            #endregion
                                            #region Declaração Prestação Servico
                                            x.Append("<DeclaracaoPrestacaoServico>\n");
                                            x.Append("<InfDeclaracaoPrestacaoServico>\n");
                                            x.Append("<Competencia>" + nf.Dt_emissao.ToString("yyyy-MM-dd") + "</Competencia>\n");
                                            #region Servico
                                            x.Append("<Servico>\n");
                                            x.Append("<Valores>\n");
                                            x.Append("<ValorServicos>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_totalProdutosServicos)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorServicos>\n");
                                            x.Append("<ValorPis>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_retidoPIS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorPis>\n");
                                            x.Append("<ValorCofins>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_retidoCofins)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorCofins>\n");
                                            x.Append("<ValorInss>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_retidoINSS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorInss>\n");
                                            x.Append("<ValorIr>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_retidoIRRF)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorIr>\n");
                                            x.Append("<ValorCsll>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_retidoCSLL)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorCsll>\n");
                                            x.Append("<ValorIss>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Vl_ISS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</ValorIss>\n");
                                            x.Append("<Aliquota>" + Convert.ToDecimal(string.Format("{0:N2}", nf.Pc_aliquotaISS)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</Aliquota>\n");
                                            x.Append("<IssRetido>" + (nf.Vl_ISS_Retido > decimal.Zero ? "1" : "2") + "</IssRetido>\n");
                                            x.Append("<ItemListaServico>" + nf.Id_tpservico + "</ItemListaServico>\n");
                                            x.Append("<CodigoCnae>" + nf.Cnae_fiscal + "</CodigoCnae>\n");
                                            x.Append("<Discriminacao>" + nf.Ds_produto.Trim() + "</Discriminacao>\n");
                                            x.Append("<CodigoMunicipio>" + nf.Cd_cidade_empresa.Trim() + "</CodigoMunicipio>\n");
                                            x.Append("<CodigoPais>1058</CodigoPais>\n");
                                            x.Append("<ExigibilidadeISS>1</ExigibilidadeISS>\n");
                                            x.Append("<MunicipioIncidencia>" + nf.Cd_municipioexecservico + "</MunicipioIncidencia>\n");
                                            x.Append("</Valores>\n");
                                            x.Append("</Servico>\n");
                                            #endregion
                                            #region Prestador Serviço
                                            x.Append("<Prestador>\n");
                                            x.Append("<CpfCnpj>\n");
                                            x.Append("<Cnpj>" + nf.Cnpj_empresa.SoNumero() + "</Cnpj>\n");
                                            x.Append("</CpfCnpj>\n");
                                            x.Append("<InscricaoMunicipal>" + nf.insc_municipal.SoNumero() + "</InscricaoMunicipal>\n");
                                            x.Append("</Prestador>\n");
                                            #endregion
                                            #region Tomador Servico
                                            x.Append("<Tomador>\n");
                                            x.Append("<IdentificacaoTomador>\n");
                                            x.Append("<CpfCnpj>\n");
                                            x.Append("<Cnpj>" + nf.Nr_doc_cliente.SoNumero() + "</Cnpj>\n");
                                            x.Append("</CpfCnpj>\n");
                                            x.Append("</IdentificacaoTomador>\n");
                                            x.Append("<RazaoSocial>" + nf.Nm_cliente.Trim() + "</RazaoSocial>\n");
                                            #region Endereco
                                            x.Append("<Endereco>\n");
                                            x.Append("<Endereco>" + nf.Endereco_cliente.Trim() + "</Endereco>\n");
                                            x.Append("<Numero>" + nf.Numero_cliente + "</Numero>\n");
                                            x.Append("<Bairro>" + nf.Bairro_cliente + "</Bairro>\n");
                                            x.Append("<CodigoMunicipio>" + nf.Cd_cidade_cliente + "</CodigoMunicipio>\n");
                                            x.Append("<Uf>" + nf.Uf_cliente + "</Uf>\n");
                                            x.Append("<CodigoPais>" + nf.Cd_pais_cliente + "</CodigoPais>\n");
                                            x.Append("<Cep>" + nf.Cep_cliente.SoNumero() + "</Cep>\n");
                                            x.Append("</Endereco>\n");
                                            #endregion
                                            x.Append("</Tomador>\n");
                                            #endregion
                                            x.Append("<OptanteSimplesNacional>" + (nf.Tp_regimetributario.Equals("3") ? "2" : "1") + "</OptanteSimplesNacional>\n");
                                            x.Append("<IncentivoFiscal>2</IncentivoFiscal>\n");
                                            x.Append("</InfDeclaracaoPrestacaoServico>\n");
                                            x.Append("</DeclaracaoPrestacaoServico>\n");
                                            #endregion
                                            x.Append("</InfNfse>\n");
                                            x.Append("</Nfse>\n");
                                            x.Append("</CompNfse>\n");
                                            xml = x.ToString();
                                        }
                                        else if (p.Tp_nota.Trim().ToUpper().Equals("P"))//Nota Propria
                                        {
                                            LoteNFe loteNFe = await NFOnvioDAO.GetLoteNFeAsync(p.Cd_empresa, p.Nr_lanctofiscal.ToString(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            if (loteNFe != null)
                                            {
                                                XmlDocument doc = new XmlDocument();
                                                doc.LoadXml(p.Xml_nfe);
                                                string xmlLote = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                                xmlLote += "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + doc.DocumentElement["infNFe"].Attributes["versao"].InnerText + "\">\n";
                                                xmlLote += p.Xml_nfe + "\n";
                                                xmlLote += "<protNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + doc.DocumentElement["infNFe"].Attributes["versao"].InnerText + "\">\n";
                                                xmlLote += "<infProt>\n";
                                                xmlLote += "<tpAmb>" + loteNFe.Tp_ambiente + "</tpAmb>\n";
                                                xmlLote += "<verAplic>" + loteNFe.Veraplic.Trim() + "</verAplic>\n";
                                                xmlLote += "<chNFe>" + p.Chave_acesso_nfe.Trim() + "</chNFe>\n";
                                                xmlLote += "<dhRecbto>" + loteNFe.Dt_processamento.ToString("yyyy-MM-ddTHH:mm:sszzz") + "</dhRecbto>\n";
                                                xmlLote += "<nProt>" + loteNFe.Nr_protocolo.ToString() + "</nProt>\n";
                                                xmlLote += "<digVal>" + loteNFe.Digitoverificado.Trim() + "</digVal>\n";
                                                xmlLote += "<cStat>" + loteNFe.Status.ToString() + "</cStat>\n";
                                                xmlLote += "<xMotivo>" + loteNFe.Ds_mensagem.Trim() + "</xMotivo>\n";
                                                xmlLote += "</infProt>\n";
                                                xmlLote += "</protNFe>\n";
                                                xmlLote += "</nfeProc>";
                                                xml = xmlLote;
                                            }
                                        }
                                        else xml = p.Xml_nfe;
                                        if (!string.IsNullOrWhiteSpace(xml))
                                        {
                                            RetornoONVIO ret = await DataService.EnviarArquivoAsync(emp, config.Url_dominio, Encoding.UTF8.GetBytes(xml));
                                            if (ret != null)
                                                if (ret.status.code.Trim().Equals("S1") && !string.IsNullOrWhiteSpace(ret.id))
                                                    await IntegraDAO.GravarAsync(
                                                        new IntegraONVIO
                                                        {
                                                            Cd_empresa = p.Cd_empresa,
                                                            Nr_lanctofiscal = p.Nr_lanctofiscal,
                                                            Id = ret.id,
                                                            Code = ret.status.code,
                                                            Message = ret.status.message,
                                                            St_registro = "0"
                                                        }, conexao.Split(':')[0], conexao.Split(':')[1]);
                                        }
                                    }
                                    else if (p.Code.Trim().ToUpper().Equals("S1") && !string.IsNullOrWhiteSpace(p.Id))
                                    {
                                        BatchV1Dto ret = await DataService.ConsultaIdAsync(emp, config.Url_dominio, p.Id);
                                        if (ret != null)
                                            if (ret.status.code.Trim().ToUpper().Equals("S2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1], Cd_empresa: p.Cd_empresa, Nr_lanctofiscal: p.Nr_lanctofiscal.ToString());
                                                if (ret.status.code.Trim().ToUpper() != "SA1")
                                                    lista.First().St_registro = "1";//Processado
                                                if (ret.filesExpanded.Count > 0)
                                                {
                                                    lista.First().Code = ret.filesExpanded.First().apiStatus.code;
                                                    lista.First().Message = ret.filesExpanded.First().apiStatus.message;
                                                }
                                                await IntegraDAO.GravarAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                            else if (ret.status.code.Trim().ToUpper().Equals("E2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1], Cd_empresa: p.Cd_empresa, Nr_lanctofiscal: p.Nr_lanctofiscal.ToString());
                                                await IntegraDAO.ExcluirAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                    }
                            }
                            #endregion
                            #region NFC-e
                            IEnumerable<NFCeOnvio> nFCeOnvios = await NFCeOnvioDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var p in nFCeOnvios)
                            {
                                //Buscar config empresa
                                Empresa emp = empresas.First(x => x.Cd_empresa == p.Cd_empresa);
                                bool token_valido = true;
                                if (emp.Dt_expira < DateTime.Now || string.IsNullOrWhiteSpace(emp.Token) || string.IsNullOrWhiteSpace(emp.Integration_key))
                                {
                                    try
                                    {
                                        TokenONVIO token = await DataService.GerarTokenONVIOAsync(config);
                                        if (token != null)
                                        {
                                            emp.Token = token.access_token;
                                            emp.Dt_token = token.Dt_token;
                                            emp.Expira_em = int.Parse(token.expires_in);
                                            //Gerar Key integração
                                            IntegrationKey key = await DataService.GerarKeyIntegracaoAsync(emp, config.Url_dominio, token.access_token);
                                            if (key != null)
                                            {
                                                emp.Integration_key = key.integrationKey;
                                                await EmpresaDAO.GravarAsync(emp, conexao.Split(':')[0], conexao.Split(':')[1]);
                                                token_valido = true;
                                            }
                                            else token_valido = false;
                                        }
                                        else token_valido = false;
                                    }
                                    catch { token_valido = false; }
                                }
                                if (token_valido)
                                    if (string.IsNullOrWhiteSpace(p.Code))
                                    {
                                        if (p.Xml_nfce != null)
                                        {
                                            XmlDocument doc = new XmlDocument();
                                            doc.LoadXml(p.Xml_nfce);
                                            string versao = doc.GetElementsByTagName("infNFe")[0].Attributes["versao"].InnerText;
                                            string xmlLote = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                            xmlLote += "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + versao + "\">\n";
                                            xmlLote += p.Xml_nfce + "\n";
                                            xmlLote += "<protNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + versao + "\">\n";
                                            xmlLote += "<infProt>\n";
                                            xmlLote += "<tpAmb>" + p.Tp_ambiente + "</tpAmb>\n";
                                            xmlLote += "<verAplic>" + p.VerAplic.Trim() + "</verAplic>\n";
                                            xmlLote += "<chNFe>" + p.Chave_acesso.Trim() + "</chNFe>\n";
                                            xmlLote += "<dhRecbto>" + p.Dt_processamento.ToString("yyyy-MM-ddTHH:mm:sszzz") + "</dhRecbto>\n";
                                            xmlLote += "<nProt>" + p.Nr_protocolo.ToString() + "</nProt>\n";
                                            xmlLote += "<digVal>" + p.DigVal.Trim() + "</digVal>\n";
                                            xmlLote += "<cStat>" + p.Status.ToString() + "</cStat>\n";
                                            xmlLote += "<xMotivo>" + p.Ds_mensagem.Trim() + "</xMotivo>\n";
                                            xmlLote += "</infProt>\n";
                                            xmlLote += "</protNFe>\n";
                                            xmlLote += "</nfeProc>";
                                            RetornoONVIO ret = await DataService.EnviarArquivoAsync(emp, config.Url_dominio, Encoding.UTF8.GetBytes(xmlLote.Trim()));
                                            if (ret != null)
                                                if (ret.status.code.Trim().Equals("S1") && !string.IsNullOrWhiteSpace(ret.id))
                                                    await IntegraDAO.GravarAsync(
                                                        new IntegraONVIO
                                                        {
                                                            Cd_empresa = p.Cd_empresa,
                                                            Id_nfce = p.Id_nfce,
                                                            Id = ret.id,
                                                            Code = ret.status.code,
                                                            Message = ret.status.message,
                                                            St_registro = "0"
                                                        }, conexao.Split(':')[0], conexao.Split(':')[1]);
                                        }
                                    }
                                    else if (p.Code.Trim().ToUpper().Equals("S1") && !string.IsNullOrWhiteSpace(p.Id))
                                    {
                                        BatchV1Dto ret = await DataService.ConsultaIdAsync(emp, config.Url_dominio, p.Id);
                                        if (ret != null)
                                            if (ret.status.code.Trim().ToUpper().Equals("S2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1], Cd_empresa: p.Cd_empresa, Id_nfce: p.Id_nfce.ToString());
                                                if (ret.status.code.Trim().ToUpper() != "SA1")
                                                    lista.First().St_registro = "1";//Processado
                                                if (ret.filesExpanded.Count > 0)
                                                {
                                                    lista.First().Code = ret.filesExpanded.First().apiStatus.code;
                                                    lista.First().Message = ret.filesExpanded.First().apiStatus.message;
                                                }
                                                await IntegraDAO.GravarAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                            else if (ret.status.code.Trim().ToUpper().Equals("E2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1], Cd_empresa: p.Cd_empresa, Id_nfce: p.Id_nfce.ToString());
                                                await IntegraDAO.ExcluirAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                    }
                            }
                            #endregion
                            #region CTe
                            IEnumerable<CTeOnvio> cTeOnvios = await CTeOnvioDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var p in cTeOnvios)
                            {
                                //Buscar config empresa
                                Empresa emp = empresas.First(x => x.Cd_empresa == p.Cd_empresa);
                                bool token_valido = true;
                                if (emp.Dt_expira < DateTime.Now || string.IsNullOrWhiteSpace(emp.Token) || string.IsNullOrWhiteSpace(emp.Integration_key))
                                {
                                    try
                                    {
                                        TokenONVIO token = await DataService.GerarTokenONVIOAsync(config);
                                        if (token != null)
                                        {
                                            emp.Token = token.access_token;
                                            emp.Dt_token = token.Dt_token;
                                            emp.Expira_em = int.Parse(token.expires_in);
                                            //Gerar Key integração
                                            IntegrationKey key = await DataService.GerarKeyIntegracaoAsync(emp, config.Url_dominio, token.access_token);
                                            if (key != null)
                                            {
                                                emp.Integration_key = key.integrationKey;
                                                await EmpresaDAO.GravarAsync(emp, conexao.Split(':')[0], conexao.Split(':')[1]);
                                                token_valido = true;
                                            }
                                            else token_valido = false;
                                        }
                                        else token_valido = false;
                                    }
                                    catch { token_valido = false; }
                                }
                                if (token_valido)
                                    if (string.IsNullOrWhiteSpace(p.Code))
                                    {
                                        if (!string.IsNullOrWhiteSpace(p.Xml_cte))
                                        {
                                            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                                            doc.LoadXml(p.Xml_cte);
                                            string versao = doc.GetElementsByTagName("infCte")[0].Attributes["versao"].InnerText;
                                            string xmlLote = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                            xmlLote += "<cteProc xmlns=\"http://www.portalfiscal.inf.br/cte\" versao=\"" + versao.Trim() + "\">\n";
                                            xmlLote += p.Xml_cte + "\n";
                                            xmlLote += "<protCTe xmlns=\"http://www.portalfiscal.inf.br/cte\" versao=\"" + versao.Trim() + "\">\n";
                                            xmlLote += "<infProt Id=\"ID" + p.Nr_protocolo.ToString() + "\">\n";
                                            xmlLote += "<tpAmb>1</tpAmb>\n";
                                            xmlLote += "<verAplic>" + p.Veraplic.Trim() + "</verAplic>\n";
                                            xmlLote += "<chCTe>" + p.Chaveacesso.Trim() + "</chCTe>\n";
                                            xmlLote += "<dhRecbto>" + p.Dt_processamento.ToString("yyyy-MM-ddTHH:MM:sszzz") + "</dhRecbto>\n";
                                            xmlLote += "<nProt>" + p.Nr_protocolo.ToString() + "</nProt>\n";
                                            xmlLote += "<digVal>" + p.Digval.Trim() + "</digVal>\n";
                                            xmlLote += "<cStat>" + p.Status_cte.ToString() + "</cStat>\n";
                                            xmlLote += "<xMotivo>" + p.Msg_status.Trim() + "</xMotivo>\n";
                                            xmlLote += "</infProt>\n";
                                            xmlLote += "</protCTe>\n";
                                            xmlLote += "</cteProc>";
                                            RetornoONVIO ret = await DataService.EnviarArquivoAsync(emp, config.Url_dominio, Encoding.UTF8.GetBytes(xmlLote.Trim()));
                                            if (ret != null)
                                                if (ret.status.code.Trim().Equals("S1") && !string.IsNullOrWhiteSpace(ret.id))
                                                    await IntegraDAO.GravarAsync(
                                                        new IntegraONVIO
                                                        {
                                                            Cd_empresa = p.Cd_empresa,
                                                            Nr_lanctoCTR = p.Nr_lanctoctr,
                                                            Id = ret.id,
                                                            Code = ret.status.code,
                                                            Message = ret.status.message,
                                                            St_registro = "0"
                                                        }, conexao.Split(':')[0], conexao.Split(':')[1]);
                                        }
                                    }
                                    else if (p.Code.Trim().ToUpper().Equals("S1") && !string.IsNullOrWhiteSpace(p.Id))
                                    {
                                        BatchV1Dto ret = await DataService.ConsultaIdAsync(emp, config.Url_dominio, p.Id);
                                        if (ret != null)
                                            if (ret.status.code.Trim().ToUpper().Equals("S2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1], Cd_empresa: p.Cd_empresa, Nr_lanctoctr: p.Nr_lanctoctr.ToString());
                                                if (ret.status.code.Trim().ToUpper() != "SA1")
                                                    lista.First().St_registro = "1";//Processado
                                                if (ret.filesExpanded.Count > 0)
                                                {
                                                    lista.First().Code = ret.filesExpanded.First().apiStatus.code;
                                                    lista.First().Message = ret.filesExpanded.First().apiStatus.message;
                                                }
                                                await IntegraDAO.GravarAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                            else if (ret.status.code.Trim().ToUpper().Equals("E2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1], Cd_empresa: p.Cd_empresa, Nr_lanctoctr: p.Nr_lanctoctr.ToString());
                                                await IntegraDAO.ExcluirAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                    }
                            }
                            #endregion
                            #region Pagamentos/Recebimentos
                            IEnumerable<BaixaOnvio> baixaOnvios = await BaixaOnvioDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var p in baixaOnvios)
                            {
                                //Buscar config empresa
                                Empresa emp = empresas.First(x => x.Cd_empresa == p.Cd_empresa);
                                bool token_valido = true;
                                if (emp.Dt_expira < DateTime.Now || string.IsNullOrWhiteSpace(emp.Token) || string.IsNullOrWhiteSpace(emp.Integration_key))
                                {
                                    try
                                    {
                                        TokenONVIO token = await DataService.GerarTokenONVIOAsync(config);
                                        if (token != null)
                                        {
                                            emp.Token = token.access_token;
                                            emp.Dt_token = token.Dt_token;
                                            emp.Expira_em = int.Parse(token.expires_in);
                                            //Gerar Key integração
                                            IntegrationKey key = await DataService.GerarKeyIntegracaoAsync(emp, config.Url_dominio, token.access_token);
                                            if (key != null)
                                            {
                                                emp.Integration_key = key.integrationKey;
                                                await EmpresaDAO.GravarAsync(emp, conexao.Split(':')[0], conexao.Split(':')[1]);
                                                token_valido = true;
                                            }
                                            else token_valido = false;
                                        }
                                        else token_valido = false;
                                    }
                                    catch { token_valido = false; }
                                }
                                if (token_valido)
                                    if (string.IsNullOrWhiteSpace(p.Code))
                                    {
                                        //Montar XML Baixa
                                        StringBuilder xml = new StringBuilder();
                                        xml.AppendLine("<?xml version=\"1.0\"?>");
                                        xml.AppendLine("<Baixas>");
                                        xml.AppendLine("<infBaixas versao=\"1.00\">");
                                        xml.AppendLine("<parcela>");
                                        xml.AppendLine("<cnpj>" + p.Nr_doctoempresa.SoNumero() + "</cnpj>");
                                        xml.AppendLine("<tipo>" + (p.Tp_docto.Trim().ToUpper().Equals("NFS-E") ? "4" : p.Tp_mov.Trim().ToUpper().Equals("P") ? "1" : "2") + "</tipo>");
                                        xml.AppendLine("<especie>" + (p.Tp_docto.Trim().ToUpper().Equals("NFS-E") ? "39" : p.Tp_docto.Trim().ToUpper().Equals("NF-E") ? "36" : "03") + "</especie>");
                                        xml.AppendLine("<serie>" + p.Nr_serie + "</serie>");
                                        xml.AppendLine("<subserie/>");
                                        xml.AppendLine("<numero>" + p.Nr_notafiscal.ToString() + "</numero>");
                                        xml.AppendLine("<datavencimento>" + p.Dt_vencto.ToString("yyyy-MM-dd") + "</datavencimento>");
                                        xml.AppendLine("<datapagamento>" + p.Dt_liquidacao.ToString("yyyy-MM-dd") + "</datapagamento>");
                                        xml.AppendLine("<valorrecebido>" + Convert.ToDecimal(string.Format("{0:N2}", p.Vl_liquidacao + p.Vl_juroacrescimo - p.Vl_descontobonus)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</valorrecebido>");
                                        xml.AppendLine("<juros>" + Convert.ToDecimal(string.Format("{0:N2}", p.Vl_juroacrescimo)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</juros>");
                                        xml.AppendLine("<multa>0</multa>");
                                        xml.AppendLine("<desconto>" + Convert.ToDecimal(string.Format("{0:N2}", p.Vl_descontobonus)).ToString(new System.Globalization.CultureInfo("en-US", true)) + "</desconto>");
                                        xml.AppendLine("<outras>0</outras>");
                                        xml.AppendLine("<fornecedor>" + (!p.Tp_docto.Trim().ToUpper().Equals("NFS-E") && p.Tp_mov.Trim().ToUpper().Equals("P") ? p.Nr_doctoclifor.SoNumero() : string.Empty) + "</fornecedor>");
                                        xml.AppendLine("<historico>" + p.Ds_historico.Trim() + "</historico>");
                                        xml.AppendLine("<titulo>" + p.Cd_parcela.ToString().PadLeft(3, '0') + "</titulo>");
                                        xml.AppendLine("</parcela>");
                                        xml.AppendLine("</infBaixas>");
                                        xml.AppendLine("</Baixas>");
                                        RetornoONVIO ret = await DataService.EnviarArquivoAsync(emp, config.Url_dominio, Encoding.UTF8.GetBytes(xml.ToString()));
                                        if (ret != null)
                                            if (ret.status.code.Trim().Equals("S1") && !string.IsNullOrWhiteSpace(ret.id))
                                                await IntegraDAO.GravarAsync(
                                                    new IntegraONVIO
                                                    {
                                                        Cd_empresa = p.Cd_empresa,
                                                        Nr_lancto = p.Nr_lancto,
                                                        Cd_parcela = p.Cd_parcela,
                                                        Id_liquid = p.Id_liquid,
                                                        Id = ret.id,
                                                        Code = ret.status.code,
                                                        Message = ret.status.message,
                                                        St_registro = "0"
                                                    }, conexao.Split(':')[0], conexao.Split(':')[1]);
                                    }
                                    else if ((p.Code.Trim().ToUpper().Equals("S1") || p.Code.Trim().ToUpper().Equals("SA1")) && !string.IsNullOrWhiteSpace(p.Id))
                                    {
                                        BatchV1Dto ret = await DataService.ConsultaIdAsync(emp, config.Url_dominio, p.Id);
                                        if (ret != null)
                                            if (ret.status.code.Trim().ToUpper().Equals("S2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0],
                                                                                        conexao.Split(':')[1],
                                                                                        Cd_empresa: p.Cd_empresa,
                                                                                        Nr_lancto: p.Nr_lancto.ToString(),
                                                                                        Cd_parcela: p.Cd_parcela.ToString(),
                                                                                        Id_liquid: p.Id_liquid.ToString());
                                                if (ret.status.code.Trim().ToUpper() != "SA1")
                                                    lista.First().St_registro = "1";//Processado
                                                if (ret.filesExpanded.Count > 0)
                                                {
                                                    lista.First().Code = ret.filesExpanded.First().apiStatus.code;
                                                    lista.First().Message = ret.filesExpanded.First().apiStatus.message;
                                                }
                                                await IntegraDAO.GravarAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                            else if (ret.status.code.Trim().ToUpper().Equals("E2"))
                                            {
                                                var lista = await IntegraDAO.GetAsync(conexao.Split(':')[0],
                                                                                        conexao.Split(':')[1],
                                                                                        Cd_empresa: p.Cd_empresa,
                                                                                        Nr_lancto: p.Nr_lancto.ToString(),
                                                                                        Cd_parcela: p.Cd_parcela.ToString(),
                                                                                        Id_liquid: p.Id_liquid.ToString());
                                                await IntegraDAO.ExcluirAsync(lista.First(), conexao.Split(':')[0], conexao.Split(':')[1]);
                                            }
                                    }
                            }
                            #endregion
                            //System.Threading.Thread.Sleep(28800000);
                            System.Threading.Thread.Sleep(15000);
                        }
                        catch { }
                    }
                    while (true);
                }
                if (!string.IsNullOrWhiteSpace(config.Url_sieg) &&
                    !string.IsNullOrWhiteSpace(config.Email_sieg) &&
                    !string.IsNullOrWhiteSpace(config.Key_sieg))
                {
                    do
                    {
                        try
                        {
                            #region NF-e
                            IEnumerable<NFeSieg> nFeSiegs = await NFeSiegDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var v in nFeSiegs)
                            {
                                try
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(v.Xml_nfe);
                                    string xmlLote = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                    xmlLote += "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + doc.DocumentElement["infNFe"].Attributes["versao"].InnerText + "\">\n";
                                    xmlLote += v.Xml_nfe + "\n";
                                    xmlLote += "<protNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + doc.DocumentElement["infNFe"].Attributes["versao"].InnerText + "\">\n";
                                    xmlLote += "<infProt>\n";
                                    xmlLote += "<tpAmb>" + v.Tp_ambiente + "</tpAmb>\n";
                                    xmlLote += "<verAplic>" + v.Veraplic.Trim() + "</verAplic>\n";
                                    xmlLote += "<chNFe>" + v.Chave_acesso_nfe.Trim() + "</chNFe>\n";
                                    xmlLote += "<dhRecbto>" + v.Dt_processamento.ToString("yyyy-MM-ddTHH:mm:sszzz") + "</dhRecbto>\n";
                                    xmlLote += "<nProt>" + v.Nr_protocolo.ToString() + "</nProt>\n";
                                    xmlLote += "<digVal>" + v.Digitoverificado.ToString() + "</digVal>\n";
                                    xmlLote += "<cStat>" + v.Status.ToString() + "</cStat>\n";
                                    xmlLote += "<xMotivo>" + v.Ds_mensagem.Trim() + "</xMotivo>\n";
                                    xmlLote += "</infProt>\n";
                                    xmlLote += "</protNFe>\n";
                                    xmlLote += "</nfeProc>";
                                    RetornoSieg ret = await DataService.PostXMLSiegAsync(config.Url_sieg,
                                                                                            config.Key_sieg,
                                                                                            config.Email_sieg,
                                                                                            xmlLote);
                                    if (ret == null ? false : !string.IsNullOrWhiteSpace(ret.Message) && ret.Error == null)
                                        await NFeSiegDAO.GravarAsync(v, conexao.Split(':')[0], conexao.Split(':')[1]);
                                }
                                catch { }
                            }
                            #endregion
                            #region Buscar Cancelamento NF-e enviar
                            IEnumerable<CancelamentoNFeSieg> cancelamentoNFeSiegs = await CancelamentoNFeSiegDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var v in cancelamentoNFeSiegs)
                            {
                                try
                                {
                                    StringBuilder xml = new StringBuilder();
                                    xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                                    xml.Append("<procEventoNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + v.Cd_versao.Trim() + "\">");
                                    xml.Append(v.Xml_evento.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty).Replace(" xmlns=\"http://www.portalfiscal.inf.br/nfe\"", string.Empty));
                                    xml.Append(v.Xml_retevento.Replace(" xmlns=\"http://www.portalfiscal.inf.br/nfe\"", string.Empty));
                                    xml.Append("</procEventoNFe>");
                                    RetornoSieg ret = await DataService.PostXMLSiegAsync(config.Url_sieg,
                                                                                            config.Key_sieg,
                                                                                            config.Email_sieg,
                                                                                            xml.ToString());
                                    if (ret == null ? false : !string.IsNullOrWhiteSpace(ret.Message) && ret.Error == null)
                                        await CancelamentoNFeSiegDAO.GravarAsync(v, conexao.Split(':')[0], conexao.Split(':')[1]);
                                }
                                catch { }
                            }
                            #endregion
                            #region NFC-e
                            IEnumerable<NFCeSieg> nFCeSiegs = await NFCeSiegDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var v in nFCeSiegs)
                            {
                                try
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(v.Xml_nfce);
                                    string versao = doc.GetElementsByTagName("infNFe")[0].Attributes["versao"].InnerText;
                                    string xmlLote = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                    xmlLote += "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + versao + "\">\n";
                                    xmlLote += v.Xml_nfce + "\n";
                                    xmlLote += "<protNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + versao + "\">\n";
                                    xmlLote += "<infProt>\n";
                                    xmlLote += "<tpAmb>" + v.Tp_ambiente + "</tpAmb>\n";
                                    xmlLote += "<verAplic>" + v.Veraplic.Trim() + "</verAplic>\n";
                                    xmlLote += "<chNFe>" + v.Chave_acesso.Trim() + "</chNFe>\n";
                                    xmlLote += "<dhRecbto>" + v.Dt_processamento.ToString("yyyy-MM-ddTHH:mm:sszzz") + "</dhRecbto>\n";
                                    xmlLote += "<nProt>" + v.Nr_protocolo.ToString() + "</nProt>\n";
                                    xmlLote += "<digVal>" + v.Digval.Trim() + "</digVal>\n";
                                    xmlLote += "<cStat>" + v.Status.ToString() + "</cStat>\n";
                                    xmlLote += "<xMotivo>" + v.Ds_mensagem.Trim() + "</xMotivo>\n";
                                    xmlLote += "</infProt>\n";
                                    xmlLote += "</protNFe>\n";
                                    xmlLote += "</nfeProc>";
                                    RetornoSieg ret = await DataService.PostXMLSiegAsync(config.Url_sieg,
                                                                                            config.Key_sieg,
                                                                                            config.Email_sieg,
                                                                                            xmlLote);
                                    if (ret == null ? false : !string.IsNullOrWhiteSpace(ret.Message) && ret.Error == null)
                                        await NFCeSiegDAO.GravarAsync(v, conexao.Split(':')[0], conexao.Split(':')[1]);
                                }
                                catch { }
                            }
                            #endregion
                            #region Buscar Cancelamento NFC-e enviar
                            IEnumerable<CancelamentoNFCeSieg> cancelamentoNFCeSiegs = await CancelamentoNFCeSiegDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var v in cancelamentoNFCeSiegs)
                            {
                                try
                                {
                                    StringBuilder xml = new StringBuilder();
                                    xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                                    xml.Append("<procEventoNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + v.Cd_versao.Trim() + "\">");
                                    xml.Append(v.Xml_evento.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty).Replace(" xmlns=\"http://www.portalfiscal.inf.br/nfe\"", string.Empty));
                                    xml.Append(v.Xml_retevento.Replace(" xmlns=\"http://www.portalfiscal.inf.br/nfe\"", string.Empty));
                                    xml.Append("</procEventoNFe>");
                                    RetornoSieg ret = await DataService.PostXMLSiegAsync(config.Url_sieg,
                                                                                            config.Key_sieg,
                                                                                            config.Email_sieg,
                                                                                            xml.ToString());
                                    if (ret == null ? false : !string.IsNullOrWhiteSpace(ret.Message) && ret.Error == null)
                                        await CancelamentoNFCeSiegDAO.GravarAsync(v, conexao.Split(':')[0], conexao.Split(':')[1]);
                                }
                                catch { }
                            }
                            #endregion
                            #region CT-e
                            IEnumerable<CTeSieg> cTeSiegs = await CTeSiegDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var v in cTeSiegs)
                            {
                                try
                                {
                                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                                    doc.LoadXml(v.Xml_cte);
                                    string versao = doc.GetElementsByTagName("infNFe")[0].Attributes["versao"].InnerText;
                                    string xmlLote = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                    xmlLote += "<cteProc xmlns=\"http://www.portalfiscal.inf.br/cte\" versao=\"" + versao.Trim() + "\">\n";
                                    xmlLote += v.Xml_cte + "\n";
                                    xmlLote += "<protCTe xmlns=\"http://www.portalfiscal.inf.br/cte\" versao=\"" + versao.Trim() + "\">\n";
                                    xmlLote += "<infProt Id=\"ID" + v.Nr_protocolo.ToString() + "\">\n";
                                    xmlLote += "<tpAmb>1</tpAmb>\n";
                                    xmlLote += "<verAplic>" + v.VerAplic.Trim() + "</verAplic>\n";
                                    xmlLote += "<chCTe>" + v.ChaveAcesso.Trim() + "</chCTe>\n";
                                    xmlLote += "<dhRecbto>" + v.Dt_processamento.ToString("yyyy-MM-ddTHH:MM:sszzz") + "</dhRecbto>\n";
                                    xmlLote += "<nProt>" + v.Nr_protocolo.ToString() + "</nProt>\n";
                                    xmlLote += "<digVal>" + v.DigVal.Trim() + "</digVal>\n";
                                    xmlLote += "<cStat>" + v.Status_cte.ToString() + "</cStat>\n";
                                    xmlLote += "<xMotivo>" + v.Msg_stauts.Trim() + "</xMotivo>\n";
                                    xmlLote += "</infProt>\n";
                                    xmlLote += "</protCTe>\n";
                                    xmlLote += "</cteProc>";
                                    RetornoSieg ret = await DataService.PostXMLSiegAsync(config.Url_sieg,
                                                                                            config.Key_sieg,
                                                                                            config.Email_sieg,
                                                                                            xmlLote);
                                    if (ret == null ? false : !string.IsNullOrWhiteSpace(ret.Message) && ret.Error == null)
                                        await CTeSiegDAO.GravarAsync(v, conexao.Split(':')[0], conexao.Split(':')[1]);
                                }
                                catch { }
                            }
                            #endregion
                            #region Cancelamento CT-e
                            IEnumerable<CancelamentoCTeSieg> cancelamentoCTeSiegs = await CancelamentoCTeSiegDAO.GetAsync(conexao.Split(':')[0], conexao.Split(':')[1]);
                            foreach (var v in cancelamentoCTeSiegs)
                            {
                                try
                                {
                                    try
                                    {
                                        XmlDocument docevent = new XmlDocument();
                                        docevent.LoadXml(v.Xml_evento);
                                        string versao = docevent.LastChild.Attributes.GetNamedItem("versao").InnerText;
                                        StringBuilder xml = new StringBuilder();
                                        xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                                        xml.Append("<procEventoCTe xmlns=\"http://www.portalfiscal.inf.br/cte\" versao=\"" + versao.Trim() + "\">");
                                        xml.Append(v.Xml_evento.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty).Replace(" xmlns=\"http://www.portalfiscal.inf.br/cte\"", string.Empty));
                                        xml.Append(v.Xml_retevent.Replace(" xmlns=\"http://www.portalfiscal.inf.br/cte\"", string.Empty));
                                        xml.Append("</procEventoCTe>");
                                        RetornoSieg ret = await DataService.PostXMLSiegAsync(config.Url_sieg,
                                                                                                config.Key_sieg,
                                                                                                config.Email_sieg,
                                                                                                xml.ToString());
                                        if (ret == null ? false : !string.IsNullOrWhiteSpace(ret.Message) && ret.Error == null)
                                            await CancelamentoCTeSiegDAO.GravarAsync(v, conexao.Split(':')[0], conexao.Split(':')[1]);
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                            #endregion
                            System.Threading.Thread.Sleep(28800000);
                        }
                        catch { }
                    }
                    while (true);
                }
            }
        }

        public void Stop()
        {
            
        }
    }
}
