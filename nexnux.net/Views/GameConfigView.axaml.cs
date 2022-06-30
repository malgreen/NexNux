using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using nexnux.net.ViewModels;
using ReactiveUI;

namespace nexnux.net.Views
{
    public partial class GameConfigView : ReactiveWindow<GameConfigViewModel>
    {
        public GameConfigView()
        {
            InitializeComponent();
            //this.WhenActivated(b => b(ViewModel!.CancelCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.SaveGameCommand.Subscribe(Close)));
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            // I think that handling this any other way would be over-engineering, as this does not really need to be tested
            Close();
        }
    }
}
