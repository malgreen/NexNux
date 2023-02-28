using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using NexNux.Models;
using NexNux.ViewModels;
using ReactiveUI;

namespace NexNux.Views;

public partial class GameListView : ReactiveWindow<GameListViewModel>
{
    public GameListView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowConfigDialog.RegisterHandler(DoShowGameConfigDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowRemoveDialog.RegisterHandler(DoShowGameRemoveDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowHomeView.RegisterHandler(DoShowHomeView)));
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task DoShowGameConfigDialogAsync(InteractionContext<GameConfigViewModel, Game?> interactionContext)
    {
        GameConfigView dialog = new GameConfigView();
        dialog.DataContext = interactionContext.Input;

        Game? result = await dialog.ShowDialog<Game>(this);
        interactionContext.SetOutput(result);
    }

    private async Task DoShowGameRemoveDialogAsync(InteractionContext<Game, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(
            "Are you sure?",
            $"This will also delete ALL mods and settings previously configured for {interactionContext.Input} within NexNux",
            ButtonEnum.OkCancel,
            MessageBox.Avalonia.Enums.Icon.Warning
        );
        var result = await messageBox.Show(this);
        interactionContext.SetOutput(result == ButtonResult.Ok);
    }

    private Task DoShowHomeView(InteractionContext<HomeViewModel, bool> interactionContext)
    {
        HomeView homeView = new HomeView();
        homeView.DataContext = interactionContext.Input;
        homeView.Show();
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = homeView;
        }

        interactionContext.SetOutput(true); // maybe this should not be an interaction
        Close();
        return Task.CompletedTask;
    }
}