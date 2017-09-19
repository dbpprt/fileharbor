using System;
using System.Runtime.Serialization;

namespace Fileharbor.Exceptions
{
    public abstract class FileharborException : Exception
    {
        public int StatusCode { get; }

        protected FileharborException(int statusCode)
        {
            StatusCode = statusCode;
        }

        protected FileharborException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        protected FileharborException(int statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        protected FileharborException(int statusCode, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            StatusCode = statusCode;
        }
    }
}
