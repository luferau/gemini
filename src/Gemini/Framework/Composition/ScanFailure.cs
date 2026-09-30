using System;

namespace Gemini.Framework.Composition
{
    /// <summary>
    /// One assembly that module discovery could not use, together with the reason the loader gave.
    /// Scanning reports these instead of aborting the application: a deployment folder that holds
    /// a third-party assembly with an unresolvable dependency must not stop the shell from starting.
    /// </summary>
    public class ScanFailure
    {
        public ScanFailure(string source, Exception error)
        {
            Source = source;
            Error = error;
        }

        /// <summary>Assembly display name, or the file path when the file could not even be read.</summary>
        public string Source { get; }

        public Exception Error { get; }

        public override string ToString()
        {
            return Source + ": " + (Error == null ? "(no error reported)" : Error.GetType().Name + ": " + Error.Message);
        }
    }
}
