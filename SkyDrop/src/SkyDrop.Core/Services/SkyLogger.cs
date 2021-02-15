using System;
using System.Diagnostics;

// Exposing SkyLogger and ILog to the entire namespace, because it is using for app output traces
namespace SkyDrop
{
    public class SkyLogger : ILog
    {
        public SkyLogger()
        {
        }


        public void _TraceInternal(string message) { }

        public void Exception (Exception exception)
        {
            this.Trace($"[{nameof(SkyLogger)}] Logging exception");

            this.Trace(exception.Message);
            this.Trace(exception.StackTrace);
        }
    }

    public interface ILog
    {
        /// <summary>
        /// _TraceInternal() should not be used, please use the Trace() extension from ILogExtensions below.
        /// </summary>
        [Obsolete]
        public void _TraceInternal(string message);

        public void Exception(Exception exception);
    }

    // Workaround for using the System.Diagnostics.Conditional attribute on ILog instance, from https://stackoverflow.com/a/39137495/9436321
    public static class ILogExtensions
    {
        [Conditional("DEBUG")]
        public static void Trace<T>(this T t, string message) where T : ILog
        {
            // Disable warning for using internal logging message - this is the one place it's ok to use
#pragma warning disable 612
            Console.WriteLine(($"[{nameof(SkyLogger)}] " + message));
#pragma warning restore 612
        }
    }
}
