using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NexNux.ViewModels;

namespace NexNux.Views;

public partial class HomeView : ReactiveWindow<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }    
}