using System;

namespace Gemini.Framework.Themes
{
    /// <summary>
    /// Reports a theme resource dictionary that could not be loaded. A theme merges dictionaries
    /// from other assemblies by <c>pack://</c> URI, so it can fail for the same deployment reason
    /// module discovery can: the referenced assembly cannot resolve one of its dependencies.
    /// </summary>
    public class ThemeResourceFailureEventArgs : EventArgs
    {
        public ThemeResourceFailureEventArgs(Uri uri, Exception error)
        {
            Uri = uri;
            Error = error;
        }

        public Uri Uri { get; }

        public Exception Error { get; }
    }
}
