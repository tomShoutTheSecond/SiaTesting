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
        public void Debug(string message) => Print(message);


        // SkyLogger

        public void Error(string errorMessage, Exception ex)
        {
            Error(errorMessage);
            Exception(ex);
        }

        public void Error(string errorMessage) => Print(errorMessage);

        protected long exceptionCount = 0;

        public void Exception(System.Exception ex)
        {
            if (ex == null)
            {
                Error("ex == null");
                return;
            }

            exceptionCount++;

            PrintError($"Encoutered exception no# {exceptionCount}");

            PrintExceptionInfo(ex, isInnerException: false);
        }

        protected void PrintExceptionInfo(System.Exception ex, bool isInnerException)
        {
            if (isInnerException)
            {
                PrintError("Logging exception - the inner exception");
            }

            PrintError(ex);

            if (ex.InnerException != null)
                PrintExceptionInfo(ex.InnerException, isInnerException: true);
        }

        private void PrintError(string message, bool printIf = true)
        {
            Print("[ERROR] " + message, printIf);
        }


        private void PrintError(object value, bool printIf = true)
        {
            PrintError(value.ToString(), printIf);
        }

        private void Print(string message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            // can improve message?
            var fileName = Path.GetFileName(sourceFilePath);

            Trace.WriteLine($"{fileName}:{sourceLineNumber} {message}");
        }

        private void Print(object value, bool printIf = true)
        {
            Print(value.ToString(), printIf);
        }
    }
}
