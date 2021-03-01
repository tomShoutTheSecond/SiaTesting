using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using SkyDrop.Core.Services;

// Tooling can in exceptional cases be placed into the root namespace, to gain accessibility wherever we need the extensions.
namespace SkyDrop
{
    public static class TraceLog
    {
        // help from https://stackoverflow.com/a/39137495/9436321

#pragma warning disable 612
        [Conditional("DEBUG")]
        public static void Trace(this ILog t, string message,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Print(message, sourceFilePath, sourceLineNumber);
        }
#pragma warning restore 612

        public static void Print(string message, string sourceFilePath, int sourceLineNumber)
        {
            string fileName = Path.GetFileName(sourceFilePath);

            Debug.WriteLine(($"{fileName}:{sourceLineNumber} " + message));
        }
    }
}