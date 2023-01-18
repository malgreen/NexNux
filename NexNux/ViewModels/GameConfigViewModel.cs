using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using NexNux.Models;

namespace NexNux.ViewModels;

public class GameConfigViewModel : ViewModelBase
{
    public GameConfigViewModel()
    {
        SaveGameCommand = ReactiveCommand.CreateFromTask(SaveGame);
        ChooseDeployPathCommand = ReactiveCommand.CreateFromTask(ChooseDeployPath);
        ChooseModsPathCommand = ReactiveCommand.CreateFromTask(ChooseModsPath);
        ShowErrorDialog = new Interaction<string, string>();
        ShowDeployFolderDialog = new Interaction<Unit, string>();
        ShowModsFolderDialog = new Interaction<Unit, string>();
        
        CanAddGame = false;
        StatusMessage = string.Empty;
        GameName = string.Empty;
        ModsPath = string.Empty;
        DeployPath = string.Empty;

        this.WhenAnyValue(x => x.GameName).Subscribe(_ => ValidateGameInput());
        this.WhenAnyValue(x => x.DeployPath).Subscribe(_ => ValidateGameInput());
        this.WhenAnyValue(x => x.ModsPath).Subscribe(_ => ValidateGameInput());


    }
    private string _gameName = null!;
    public string GameName
    {
        get => _gameName;
        set => this.RaiseAndSetIfChanged(ref _gameName, value);
    }
    
    private string _deployPath = null!;
    public string DeployPath
    {
        get => _deployPath;
        set => this.RaiseAndSetIfChanged(ref _deployPath, value);
    }

    private string _modsPath = null!;
    public string ModsPath
    {
        get => _modsPath;
        set => this.RaiseAndSetIfChanged(ref _modsPath, value);
    }

    private bool _canAddGame;
    public bool CanAddGame
    {
        get => _canAddGame;
        set => this.RaiseAndSetIfChanged(ref _canAddGame, value);
    }

    private string _statusMessage = null!;
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ReactiveCommand<Unit, Game?> SaveGameCommand { get; }
    public ReactiveCommand<Unit, Unit> ChooseDeployPathCommand { get; }
    public ReactiveCommand<Unit, Unit> ChooseModsPathCommand { get; }
    public Interaction<string, string> ShowErrorDialog { get; } 
    public Interaction<Unit, string> ShowDeployFolderDialog { get; }
    public Interaction<Unit, string> ShowModsFolderDialog { get; }

    public async Task<Game?> SaveGame()
    {
        try
        {
            Game game = new Game(GameName, DeployPath, ModsPath);
            return game;
        }
        catch (Exception e)
        {
            await ShowErrorDialog.Handle(e.Message);
            Debug.WriteLine(e);
            return null; // We check for null returns when opening the game config window
        }
    }

    async Task ChooseDeployPath()
    {
        DeployPath = await ShowDeployFolderDialog.Handle(Unit.Default);
    }

    async Task ChooseModsPath()
    {
        ModsPath = await ShowModsFolderDialog.Handle(Unit.Default);
    }

    void ValidateGameInput()
    {
        CanAddGame = false;
        if (string.IsNullOrWhiteSpace(GameName))
            StatusMessage = "❌ Game must have a name";
        else if (GameName.StartsWith(" "))
            StatusMessage = "❌ Game name cannot start with whitespace";
        else if (GameName.StartsWith("."))
            StatusMessage = "❌ Game name cannot start with \'.\'";
        else if (string.IsNullOrWhiteSpace(DeployPath))
            StatusMessage = "❌ Game must have a deploy directory";
        else if (string.IsNullOrWhiteSpace(ModsPath))
            StatusMessage = "❌ Game must have a mods directory";
        else if (!Directory.Exists(DeployPath))
            StatusMessage = "❌ Deploy directory does not exist";
        else if (!Directory.Exists(ModsPath))
            StatusMessage = "❌ Mods directory does not exist";
        else if (!Equals(Path.GetPathRoot(DeployPath), Path.GetPathRoot(ModsPath)))
            StatusMessage = "❌ Directories must reside on the same drive";
        else if (Directory.EnumerateFileSystemEntries(ModsPath).Any()) // This makes it so the editing a game no longer works
            StatusMessage = "❌ Mods directory must be empty";
        else
        {
            StatusMessage = "✅ Looks good";
            CanAddGame = true;
        }
    }
}