using System;
using System.Runtime.Serialization;

namespace Fileharbor.Exceptions
{
    public abstract class ApplicationException : Exception
    {
        protected ApplicationException()
        {
        }

        protected ApplicationException(string message) : base(message)
        {
        }

        protected ApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
