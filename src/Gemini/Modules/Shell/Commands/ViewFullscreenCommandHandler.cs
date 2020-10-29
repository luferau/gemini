using System.Threading.Tasks;
using System.Windows;
using Gemini.Framework.Commands;

namespace Gemini.Modules.Shell.Commands
{
    [CommandHandler]
    public class ViewFullScreenCommandHandler : CommandHandlerBase<ViewFullScreenCommandDefinition>
    {
        public override Task<bool> Run(Command command)
        {
            var window = Application.Current.MainWindow;
            if (window == null)
                return Task.FromResult(true);
            window.WindowState = window.WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
            return Task.FromResult(true);
        }
    }
}
