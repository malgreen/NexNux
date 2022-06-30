using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using nexnux.net.ViewModels;
using nexnux.net.Models;
using ReactiveUI;

namespace nexnux.net.Views
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
            var dialog = new GameConfigView();
            dialog.DataContext = interactionContext.Input;

            var result = await dialog.ShowDialog<Game>(this);
            interactionContext.SetOutput(result);
        }
    }
}
