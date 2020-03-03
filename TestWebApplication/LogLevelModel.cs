using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TestWebApplication
{
    public class LogLevelModel
    {
        public string Category { get; set; }

        public LogLevel LogLevel { get; set; }
    }
}
