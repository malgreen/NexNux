using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace nexnux.net.Views;

public partial class ModListView : Window
{
    public ModListView()
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