using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication
{
    public class BaseMessage<T>
    {
        public T Content { get; set; }
    }
}
