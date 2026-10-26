#if !MODULE_OUTPUT
using System.ComponentModel.Composition;
using System.IO;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Gemini.Modules.Output
{
    /// <summary>
    /// Stands in for the output pane in a build that leaves the Output module out. The shell, the
    /// model designer and the unit pages report through <see cref="IOutput"/>, so the contract must
    /// stay resolvable; there is no pane, and what they write is discarded.
    /// </summary>
    [Export(typeof(IOutput))]
    public sealed class SilentOutput : Tool, IOutput
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        public TextWriter Writer
        {
            get { return TextWriter.Null; }
        }

        public void AppendLine(string text)
        {
        }

        public void Append(string text)
        {
        }

        public void Clear()
        {
        }
    }
}
#endif
