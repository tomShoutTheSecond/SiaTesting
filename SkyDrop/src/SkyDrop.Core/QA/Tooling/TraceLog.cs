using System.Diagnostics;
using SkyDrop.Core.Services;

// Tooling can in exceptional cases be placed into the root namespace, to gain accessibility wherever we need the extensions.
namespace SkyDrop
{
    public static class TraceLog
    {
#pragma warning disable 612
        [Conditional("DEBUG")]
        public static void Trace<T>(this T t, string message) where T : ILog
        {
            // from https://stackoverflow.com/a/39137495/9436321
            Debug.WriteLine(($"[{nameof(SkyLogger)}] " + message));
        }
#pragma warning restore 612

    }
}
