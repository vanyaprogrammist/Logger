using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication
{
    public class LogModel
    {
        public LogData Data { get; set; }
    }

    public class LogData
    {
        public Exception Exception { get; set; }

        public string Message { get; set; }
    }
}
