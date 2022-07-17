using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Collections.Generic;
using nexnux.net.Models;
using nexnux.net.ViewModels;
using ReactiveUI;
using System;

namespace nexnux.net.Views;

public partial class ModListView : Window
{
    public ModListView()
    {
        InitializeComponent();
        this.DataContextChanged += ModListView_DataContextChanged;
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

    private void ModListView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ModListViewModel modListViewModel)
        {
            Title = "NexNux - " + modListViewModel.CurrentGame.ToString();
        }
    }
}