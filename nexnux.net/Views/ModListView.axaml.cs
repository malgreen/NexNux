using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Collections.Generic;
using nexnux.net.Models;
using nexnux.net.ViewModels;
using ReactiveUI;

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

    void StartDrag()
    {

    }
    void EndDrag()
    {
       
    }
}