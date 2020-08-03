using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubUpdater
{
    public class ExceptionEventArgs<T> : EventArgs where T : Exception
    {
        /// <summary>
        /// The original exception
        /// </summary>
        public T OriginalException { get; private set; }
        /// <summary>
        /// The exception message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="originalException">The original exception</param>
        public ExceptionEventArgs(T originalException)
        {
            OriginalException = originalException;
        }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="originalException">The original exception</param>
        /// <param name="message">The exception message</param>
        public ExceptionEventArgs(T originalException, string message) : this(originalException)
        {
            Message = message;
        }
    }
}
