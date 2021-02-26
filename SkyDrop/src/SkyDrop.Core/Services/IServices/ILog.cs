using System;
using System.Runtime.CompilerServices;

namespace SkyDrop.Core.Services
{
    public interface ILog
    {
        public void Exception(Exception exception,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Error(string errorMessage, Exception ex,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Error(string errorMessage,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);
    }
}
