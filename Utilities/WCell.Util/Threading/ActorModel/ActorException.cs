using System;
using System.Runtime.Serialization;

namespace WCell.Util.Threading.ActorModel
{
    [Serializable]
    public class ActorException : Exception
    {
        public ActorException()
        {
        }

        public ActorException(string message)
            : base(message)
        {
        }

        public ActorException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ActorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
