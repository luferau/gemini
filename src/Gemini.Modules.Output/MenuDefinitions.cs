using System.ComponentModel.Composition;
using Gemini.Framework.Menus;
using Gemini.Modules.Output.Commands;

namespace Gemini.Modules.Output
{
    public static class MenuDefinitions
    {
#if MODULE_OUTPUT
        [Export]
        public static MenuItemDefinition ViewOutputMenuItem = new CommandMenuItemDefinition<ViewOutputCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 1);
#endif
    }
}