using System;
using Avalonia.ReactiveUI;
using NexNux.ViewModels;
using ReactiveUI;

namespace NexNux.Views;

public partial class ModConfigView : ReactiveWindow<ModConfigViewModel>
{
    public ModConfigView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.InstallModCommand.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));
    }

}