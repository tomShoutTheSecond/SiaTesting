using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using FFImageLoading.Helpers;
using SkyDrop.Core.Services;

// Exposing SkyLogger and ILog to the entire namespace, because it is using for app output traces
namespace SkyDrop
{
    public class SkyLogger : ILog, IMiniLogger
    {
        // IMiniLogger methods for FFImageLoading logging
        public void Debug(string message) => Print(message, nameof(IMiniLogger), -1);

        public void Error(string errorMessage) => Print(errorMessage, nameof(IMiniLogger), -1);

        public void Error(string errorMessage, Exception exception) => Exception(exception, errorMessage, -1);


        // SkyLogger

        public void Error(string errorMessage, Exception ex,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Error(errorMessage);
            Exception(ex);
        }

        public void Error(string errorMessage,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) => PrintError(errorMessage, sourceFilePath, sourceLineNumber);

        protected long exceptionCount = 0;

        public void Exception(System.Exception ex,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ex == null)
            {
                Error("ex == null", sourceFilePath, sourceLineNumber);
                return;
            }

            exceptionCount++;

            //TODO: had to comment this out because it was crashing the qr code scanner in DropViewModel, not sure whats up
            PrintError($"Encoutered exception no# {exceptionCount}", sourceFilePath, sourceLineNumber);

            PrintExceptionInfo(ex, isInnerException: false, sourceFilePath, sourceLineNumber);
        }

        protected void PrintExceptionInfo(System.Exception ex, bool isInnerException, string sourceFilePath, int sourceLineNumber)
        {
            if (isInnerException)
            {
                PrintError("Logging exception - the inner exception", sourceFilePath, sourceLineNumber);
            }

            Print(ex.Message, sourceFilePath, sourceLineNumber);
            Print(ex.StackTrace, sourceFilePath, sourceLineNumber);

            if (ex.InnerException != null)
                PrintExceptionInfo(ex.InnerException, isInnerException: true, sourceFilePath, sourceLineNumber);
        }

        private void PrintError(string message,
                                string sourceFilePath,
                                int sourceLineNumber,
                                bool printIf = true)
        {
            if (printIf)
                Print($"[ERROR] {message}", sourceFilePath, sourceLineNumber);
        }

        private void Print(string message,
                           string sourceFilePath,
                           int sourceLineNumber)
        {
            TraceLog.Print(message, sourceFilePath, sourceLineNumber);
        }
    }
}
