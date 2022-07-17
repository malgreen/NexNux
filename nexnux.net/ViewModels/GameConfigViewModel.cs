using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using nexnux.net.Models;
using nexnux.net.Views;
using ReactiveUI;
using MessageBox.Avalonia;

namespace nexnux.net.ViewModels;

public class GameConfigViewModel : ViewModelBase
{
    public GameConfigViewModel()
    {
        SaveGameCommand = ReactiveCommand.CreateFromTask(SaveGame);
        ChooseDeployPathCommand = ReactiveCommand.CreateFromTask(ChooseDeployPath);
        ChooseModsPathCommand = ReactiveCommand.CreateFromTask(ChooseModsPath);
        
    }
    private string _gameName;
    public string GameName
    {
        get => _gameName;
        set => this.RaiseAndSetIfChanged(ref _gameName, value);
    }
    
    private string _deployPath;
    public string DeployPath
    {
        get => _deployPath;
        set => this.RaiseAndSetIfChanged(ref _deployPath, value);
    }

    private string _modsPath;
    public string ModsPath
    {
        get => _modsPath;
        set => this.RaiseAndSetIfChanged(ref _modsPath, value);
    }

    public ReactiveCommand<Unit, Game?> SaveGameCommand { get; }
    public ReactiveCommand<Unit, string> ChooseDeployPathCommand { get; }
    public ReactiveCommand<Unit, string> ChooseModsPathCommand { get; }

    public async Task<Game?> SaveGame()
    {
        try
        {
            Game? game = new Game(GameName, DeployPath, ModsPath);
            return game;
        }
        catch (Exception e)
        {
            // This is really not a good way to show this, but who knows how else to do it?
            // Perhaps all the error checking could be done in here, without a pop-up, but just disabling save button until it lgtm?
            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error!", e.Message, MessageBox.Avalonia.Enums.ButtonEnum.Ok);
            
            // Get active window
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await messageBox.ShowDialog(desktop.MainWindow);
            }

            //await messageBox.Show();
            Debug.WriteLine(e);
            return null; // We check for null returns when opening the game config window
        }
    }

    async Task<string> ChooseDeployPath()
    {
        string path;
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        path = await folderDialog.ShowAsync(new GameConfigView()) ?? string.Empty;
        DeployPath = path;
        return path;
    }

    async Task<string> ChooseModsPath()
    {
        string path;
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        path = await folderDialog.ShowAsync(new GameConfigView()) ?? string.Empty;
        ModsPath = path;
        return path;
    }
}