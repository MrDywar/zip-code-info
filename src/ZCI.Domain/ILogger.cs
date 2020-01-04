using System;
using System.Collections.Generic;
using System.Text;

namespace ZCI.Domain
{
    public interface ILogger
    {
        void Debug(string msg);
        void Info(string msg);
        void Error(string msg);
    }
}
