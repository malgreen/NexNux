using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using NexNux.ViewModels;
using ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;

namespace NexNux.Views;

public partial class GameConfigView : ReactiveWindow<GameConfigViewModel>
{
    public GameConfigView()
    {
        InitializeComponent();
        //this.WhenActivated(b => b(ViewModel!.CancelCommand.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.SaveGameCommand.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.ShowErrorDialog.RegisterHandler(DoShowErrorDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowDeployFolderDialog.RegisterHandler(DoShowDeployFolderDialog)));
        this.WhenActivated(d => d(ViewModel!.ShowModsFolderDialog.RegisterHandler(DoShowModsFolderDialog)));
        this.WhenActivated(d => d(ViewModel!.ShowAppDataFolderDialog.RegisterHandler(DoShowAppDataFolderDialog)));
    }

    private async Task DoShowErrorDialogAsync(InteractionContext<string, string> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error!", interactionContext.Input, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Warning);
        await messageBox.ShowDialog(this);
        interactionContext.SetOutput(""); // This has to be here, otherwise ReactiveUI is not happy, so maybe this should not be an interaction at all?
    }

    private async Task DoShowDeployFolderDialog(InteractionContext<Unit, string> interactionContext)
    {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        openFolderDialog.Title = "Choose deploy folder";
        interactionContext.SetOutput(await openFolderDialog.ShowAsync(this) ?? string.Empty);
    }

    private async Task DoShowModsFolderDialog(InteractionContext<Unit, string> interactionContext)
    {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        openFolderDialog.Title = "Choose mods folder";
        interactionContext.SetOutput(await openFolderDialog.ShowAsync(this) ?? string.Empty);
    }
    
    private async Task DoShowAppDataFolderDialog(InteractionContext<Unit, string> interactionContext)
    {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        openFolderDialog.Title = "Choose the AppData folder for this game";
        interactionContext.SetOutput(await openFolderDialog.ShowAsync(this) ?? string.Empty);
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        // I think that handling this any other way would be over-engineering, as this does not really need to be tested
        Debug.WriteLine("Game Config cancelled, sender: " + sender + ", Args:" + e);
        Close();
    }
}