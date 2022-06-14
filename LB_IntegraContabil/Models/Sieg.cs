using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB_IntegraContabil.Models
{
    public class ErroSieg
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
    public class RetornoSieg
    {
        public string Message { get; set; } = string.Empty;
        public ErroSieg Error { get; set; }
    }
}
