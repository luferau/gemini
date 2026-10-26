using System.ComponentModel.Composition;
using Gemini.Framework.Menus;
using Gemini.Modules.Toolbox.Commands;

namespace Gemini.Modules.Toolbox
{
    public static class MenuDefinitions
    {
#if MODULE_TOOLBOX
        [Export]
        public static MenuItemDefinition ViewToolboxMenuItem = new CommandMenuItemDefinition<ViewToolboxCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 4);
#endif
    }
}
