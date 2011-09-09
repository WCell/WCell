using System;
using System.Runtime.Serialization;

namespace WCell.MPQTool.StormLibWrapper
{
    [Serializable]
    public class StormLibException : Exception
    {
        private readonly int errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="StormLibException"/> class.
        /// </summary>
        public StormLibException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StormLibException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public StormLibException(int errorCode)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StormLibException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public StormLibException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StormLibException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        public StormLibException(string message, int errorCode)
            : base(message)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StormLibException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StormLibException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StormLibException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        public StormLibException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public int ErrorCode
        {
            get { return errorCode; }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        public override string Message
        {
            get
            {
                return base.Message + " Error Code: " + errorCode;
            }
        }
    }
}