using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NexNux.ViewModels;
using ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;

namespace NexNux.Views
{
    public partial class GameConfigView : ReactiveWindow<GameConfigViewModel>
    {
        public GameConfigView()
        {
            InitializeComponent();
            //this.WhenActivated(b => b(ViewModel!.CancelCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.SaveGameCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ShowErrorDialog.RegisterHandler(DoShowErrorDialogAsync)));
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowErrorDialogAsync(InteractionContext<string, string> interactionContext)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error!", interactionContext.Input, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Warning);
            await messageBox.ShowDialog(this);
            interactionContext.SetOutput(""); // This has to be here, otherwise ReactiveUI is not happy, so maybe this should not be an interaction at all?
        }

        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            // I think that handling this any other way would be over-engineering, as this does not really need to be tested
            Close();
        }
    }
}
