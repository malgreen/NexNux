using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NexNux.ViewModels;
using ReactiveUI;

namespace NexNux.Views
{
    public partial class ModConfigView : ReactiveWindow<ModConfigViewModel>
    {
        public ModConfigView()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.InstallModCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
