using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Collections.Generic;
using NexNux.Models;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using NexNux.ViewModels;

namespace NexNux.Views;

public partial class ModListView : ReactiveWindow<ModListViewModel>
{
    public ModListView()
    {
        InitializeComponent();
        this.DataContextChanged += ModListView_DataContextChanged;
        this.WhenActivated(d => d(ViewModel!.ShowModInstallDialog.RegisterHandler(DoShowModInstallDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowModUninstallDialog.RegisterHandler(DoShowModUninstallDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowErrorDialog.RegisterHandler(DoShowErrorDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowModExistsDialog.RegisterHandler(DoShowModExistsDialogAsync)));
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
            Title = "NexNux - " + modListViewModel.CurrentGame;
        }
    }

    private async Task DoShowErrorDialogAsync(InteractionContext<string, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error!", interactionContext.Input, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Warning);
        await messageBox.ShowDialog(this);
        interactionContext.SetOutput(true);
    }

    private async Task DoShowModInstallDialogAsync(InteractionContext<ModConfigViewModel, Mod?> interactionContext)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Title = "Choose mod archive",
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter()
                {
                    Extensions = new List<string> {"zip", "rar", "7z", "tar", "gzip"}
                }
            }
        };

        string[]? result = await openFileDialog.ShowAsync(this);
        if (result == null)
        {
            interactionContext.SetOutput(null);
            return;
        }

        ModConfigView dialog = new ModConfigView();
        interactionContext.Input.UpdateModArchive(string.Join("", result));
        dialog.DataContext = interactionContext.Input;

        Mod? mod = await dialog.ShowDialog<Mod>(this);
        interactionContext.SetOutput(mod);
    }
    private async Task DoShowModUninstallDialogAsync(InteractionContext<Mod, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(
            $"Uninstalling {interactionContext.Input}, are you sure?",
            $"This will also delete the files for \"{interactionContext.Input}\" from your system.", // currently this is a lie
            ButtonEnum.OkCancel,
            MessageBox.Avalonia.Enums.Icon.Warning
        );
        var result = await messageBox.Show(this);
        interactionContext.SetOutput(result == ButtonResult.Ok);
    }

    private async Task DoShowModExistsDialogAsync(InteractionContext<Mod, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(
            "Mod already exists",
            $"Mod \"{interactionContext.Input}\" already exists, continuing will merge the two while overriding existing files.", // currently this is a lie
            ButtonEnum.OkCancel,
            MessageBox.Avalonia.Enums.Icon.Info
        );
        var result = await messageBox.Show(this);
        interactionContext.SetOutput(result == ButtonResult.Ok);
    }
}