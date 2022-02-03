using System;
using System.Runtime.Serialization;

namespace HungryClient
{
    [Serializable]
    internal class UnableToConnectToServerException : Exception
    {
        public UnableToConnectToServerException()
        {
        }

        public UnableToConnectToServerException(string message) : base(message)
        {
        }

        public UnableToConnectToServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnableToConnectToServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}