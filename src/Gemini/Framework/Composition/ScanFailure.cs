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

        /// <summary>
        /// True when the loader refused the file itself rather than one of its dependencies: the
        /// signature is HResult 0x80131515 with a NotSupportedException in the chain. That is what a
        /// file Windows marks as downloaded (Mark-of-the-Web) or a remote location produces, and it is
        /// worth naming separately because the remedy is a deployment one, not a missing dependency.
        /// </summary>
        public bool IsLocationBlocked
        {
            get
            {
                var current = Error;
                var depth = 0;

                while (current != null && depth < 8)
                {
                    if (current.HResult == LoadFromRemoteSourcesHResult &&
                        (current is NotSupportedException || current.InnerException is NotSupportedException))
                        return true;

                    current = current.InnerException;
                    depth++;
                }

                return false;
            }
        }

        private const int LoadFromRemoteSourcesHResult = unchecked((int)0x80131515);

        public override string ToString()
        {
            return Source + ": " + (Error == null ? "(no error reported)" : Error.GetType().Name + ": " + Error.Message);
        }
    }
}
