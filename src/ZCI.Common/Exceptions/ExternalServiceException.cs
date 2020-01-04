using System;
using System.Collections.Generic;
using System.Text;

namespace ZCI.Common.Exceptions
{
    [Serializable]
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException()
        {
        }

        public ExternalServiceException(string message) : base(message)
        {
        }

        public ExternalServiceException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
