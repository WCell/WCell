using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StormLibWrapper
{
    /// <summary>
    /// StormLib nativ error handler.
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// Handles the return value of native StormLib calls and throws an appropriate exception when an error happens.
        /// </summary>
        /// <param name="returnValue">The return value of the StormLib call.</param>
        /// <param name="exceptionMessage">The message of the exception to throw.</param>
        /// <remarks>
        /// The <see cref="StormLibException"/> contains the <see cref="StormLibException.ErrorCode"/> of the failed call.
        /// </remarks>
        public static void ThrowOnFailure(bool returnValue, string exceptionMessage)
        {
            if (exceptionMessage == null)
                throw new ArgumentNullException("exceptionMessage");

            if (!returnValue)
            {
                throw new StormLibException(exceptionMessage, Marshal.GetLastWin32Error());
            }
        }
    }
}

