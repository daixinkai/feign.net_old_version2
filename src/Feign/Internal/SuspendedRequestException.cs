using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Internal
{
    class SuspendedRequestException : Exception
    {
        public SuspendedRequestException()
        {
        }

        public SuspendedRequestException(string message) : base(message)
        {
        }

        public SuspendedRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SuspendedRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
