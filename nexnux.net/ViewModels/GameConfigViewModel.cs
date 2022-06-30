using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using nexnux.net.Models;
using nexnux.net.Views;
using ReactiveUI;

namespace nexnux.net.ViewModels;

public class GameConfigViewModel : ViewModelBase
{
    public GameConfigViewModel()
    {
        SaveGameCommand = ReactiveCommand.Create(SaveGame);
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

    public Game SaveGame()
    {
        Game game = new Game(GameName, DeployPath, ModsPath);
        return game;
    }

    async Task<string> ChooseDeployPath()
    {
        string path = string.Empty;
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        path = await folderDialog.ShowAsync(new GameConfigView());
        DeployPath = path;
        return path;
    }

    async Task<string> ChooseModsPath()
    {
        string path = string.Empty;
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        path = await folderDialog.ShowAsync(new GameConfigView());
        ModsPath = path;
        return path;
    }
}