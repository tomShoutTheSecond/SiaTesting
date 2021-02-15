using System;
using System.Diagnostics;

namespace SkyDrop.Core.Services
{
    public class SkyLogger : ILog
    {
        public SkyLogger()
        {
        }

        public void Trace(string message) =>
            Debug.WriteLine(message);

        public void Exception (Exception exception)
        {
            string message = $"[{nameof(SkyLogger)}] Logging exception";

            Trace(exception.Message);
            Trace(exception.StackTrace);
        }
    }

    public interface ILog
    {
        public void Trace(string message);

        public void Exception(Exception exception);
    }
}
