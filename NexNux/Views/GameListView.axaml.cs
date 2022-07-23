using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NexNux.Models;
using NexNux.ViewModels;
using ReactiveUI;

namespace NexNux.Views
{
    public partial class GameListView : ReactiveWindow<GameListViewModel>
    {
        public GameListView()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowConfigDialog.RegisterHandler(DoShowDialogAsync)));

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowDialogAsync(InteractionContext<GameConfigViewModel, Game?> interactionContext)
        {
            GameConfigView dialog = new GameConfigView();
            dialog.DataContext = interactionContext.Input;

            Game? result = await dialog.ShowDialog<Game>(this);
            interactionContext.SetOutput(result);
        }
    }
}
