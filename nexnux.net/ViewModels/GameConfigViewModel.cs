using System;
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

    public ReactiveCommand<Unit, Game> SaveGameCommand { get; }
    public ReactiveCommand<Unit, string> ChooseDeployPathCommand { get; }
    public ReactiveCommand<Unit, string> ChooseModsPathCommand { get; }

    public Task<Game> SaveGame()
    {
        try
        {
            Game game = new Game(GameName, DeployPath, ModsPath);
            return Task.FromResult(game);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
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