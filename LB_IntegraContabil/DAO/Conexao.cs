using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace LB_IntegraContabil.DAO
{
    public class TConexao : IDisposable
    {
        public SqlConnection _conexao { get; private set; }
        public TConexao(string servidor, string banco)
        {
            //string servidor = ConfigurationManager.AppSettings["servidor"];
            //string banco = ConfigurationManager.AppSettings["banco"];
            _conexao = new SqlConnection("Data Source=" + servidor + ";Initial Catalog=" + banco + ";User ID=MASTER;Password=LBSystem!@#;Persist Security Info=True;");
        }

        public async Task<bool> OpenConnectionAsync()
        {
            try
            {
                await _conexao.OpenAsync();
                return true;
            }
            catch { return false; }
        }

        public void Dispose()
        {
            if (_conexao != null)
            {
                if (_conexao.State == System.Data.ConnectionState.Open)
                    _conexao.Close();
                _conexao = null;
            }
        }
    }
}
