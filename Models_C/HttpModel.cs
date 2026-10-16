using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models_C
{
    public class HttpModel
    {
        public ReturnData ReturnData { get; set; }
    }

    public class ReturnData
    {
        public string RtnString { get; set; }
        public string ErrMsg { get; set; }
    }
}
