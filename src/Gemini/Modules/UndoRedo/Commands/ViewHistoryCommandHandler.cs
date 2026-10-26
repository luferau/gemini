using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;

namespace Gemini.Modules.UndoRedo.Commands
{
#if MODULE_HISTORY
    [CommandHandler]
#endif
    public class ViewHistoryCommandHandler : CommandHandlerBase<ViewHistoryCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewHistoryCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task<bool> Run(Command command)
        {
            _shell.ShowTool<IHistoryTool>();
            return Task.FromResult(true);
        }
    }
}
