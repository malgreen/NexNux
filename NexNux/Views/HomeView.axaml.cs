using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using NexNux.ViewModels;
using Avalonia.Controls.ApplicationLifetimes;
namespace NexNux.Views;

public partial class HomeView : ReactiveWindow<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowErrorDialog.RegisterHandler(DoShowErrorDialogAsync)));
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }    
    
    private async Task DoShowErrorDialogAsync(InteractionContext<string, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error!", interactionContext.Input, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Warning);
        await messageBox.ShowDialog(GetMainWindow());
        interactionContext.SetOutput(true);
    }
    
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return lifetime.MainWindow;
        }
        return null;
    }
}