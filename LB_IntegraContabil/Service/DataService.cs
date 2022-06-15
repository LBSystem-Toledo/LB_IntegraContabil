using LB_IntegraContabil.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.Service
{
    public static class DataService
    {
        public static async Task<TokenONVIO> GerarTokenONVIOAsync(Config config)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.Url_oauth);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", "did=s%3Av0%3A145b8a90-ea57-11eb-ae8a-877f15a4a518.QhUcTCGsMP28yWAB%2BYsUUZ5Gw4Srxf%2F0IDRkKPUQQHs; did_compat=s%3Av0%3A145b8a90-ea57-11eb-ae8a-877f15a4a518.QhUcTCGsMP28yWAB%2BYsUUZ5Gw4Srxf%2F0IDRkKPUQQHs");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", config.Client_id, config.Client_secret))));
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("grant_type", "client_credentials");
            dict.Add("client_id", config.Client_id);
            dict.Add("client_secret", config.Client_secret);
            dict.Add("audience", config.Audience);
            HttpResponseMessage response = await client.PostAsync("/oauth/token", new FormUrlEncodedContent(dict));
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<TokenONVIO>(await response.Content.ReadAsStringAsync());
            else return null;
        }
        public static async Task<IntegrationKey> GerarKeyIntegracaoAsync(Empresa empresa, string Url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + empresa.Token);
            client.DefaultRequestHeaders.TryAddWithoutValidation("x-integration-key", empresa.Chave_cliente);
            HttpResponseMessage response = await client.PostAsync("/dominio/integration/v1/activation/enable", null);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<IntegrationKey>(await response.Content.ReadAsStringAsync());
            else return null;
        }
        public static async Task<RetornoONVIO> EnviarArquivoAsync(Empresa empresa, string Url, byte[] file)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + empresa.Token);
            client.DefaultRequestHeaders.TryAddWithoutValidation("x-integration-key", empresa.Integration_key);
            MultipartFormDataContent form = new MultipartFormDataContent();
            HttpContent cfile = new ByteArrayContent(file);
            cfile.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            form.Add(cfile, "file[]");

            HttpContent c = new StringContent("{\"boxeFile\": false}");
            c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            form.Add(c, "query");

            HttpResponseMessage response = await client.PostAsync("/dominio/invoice/v3/batches", form);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<RetornoONVIO>(await response.Content.ReadAsStringAsync());
            else return null;
        }
        public static async Task<BatchV1Dto> ConsultaIdAsync(Empresa empresa, string Url, string Id)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + empresa.Token);
            client.DefaultRequestHeaders.TryAddWithoutValidation("x-integration-key", empresa.Integration_key);
            HttpResponseMessage response = await client.GetAsync("/dominio/invoice/v3/batches/" + Id);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<BatchV1Dto>(await response.Content.ReadAsStringAsync());
            else return null;
        }
        public static async Task<RetornoSieg> PostXMLSiegAsync(string Url_sieg, string Key_sieg, string Email_sieg, string xml)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Url_sieg);
            var content = new StringContent(xml, Encoding.UTF8, "text/xml");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
            HttpResponseMessage response = await client.PostAsync("/aws/api-xml.ashx?apikey=" + Key_sieg.Trim() + "&email=" + Email_sieg.Trim(), content);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<RetornoSieg>(await response.Content.ReadAsStringAsync());
            else return null;

        }
    }
}
