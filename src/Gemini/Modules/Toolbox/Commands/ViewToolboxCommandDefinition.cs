using Gemini.Framework.Commands;
using Gemini.Properties;

namespace Gemini.Modules.Toolbox.Commands
{
#if MODULE_TOOLBOX
    [CommandDefinition]
#endif
    public class ViewToolboxCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Toolbox";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewToolboxCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewToolboxCommandToolTip; }
        }
    }
}