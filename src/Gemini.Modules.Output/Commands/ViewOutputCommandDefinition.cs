using Gemini.Framework.Commands;
using Gemini.Modules.Output.Properties;

namespace Gemini.Modules.Output.Commands
{
#if MODULE_OUTPUT
    [CommandDefinition]
#endif
    public class ViewOutputCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Output";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewOutputCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewOutputCommandToolTip; }
        }
    }
}