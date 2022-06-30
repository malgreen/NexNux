using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using nexnux.net.Models;
using ReactiveUI;

namespace nexnux.net.ViewModels;

public class GameListViewModel : ViewModelBase
{
    public GameListViewModel()
    {
        MainGameList = new GameList("C:\\Users\\Malgreen\\Desktop\\ExtractTest\\avalonia\\games.json");
        Games = new ObservableCollection<Game>(MainGameList.LoadList());
        ShowConfigDialog = new Interaction<GameConfigViewModel, Game?>();
        
        AddGameCommand = ReactiveCommand.CreateFromTask(AddGame);
        EditGameCommand = ReactiveCommand.CreateFromTask(EditGame);

            RemoveGameCommand = ReactiveCommand.Create(RemoveGame);
        Games.CollectionChanged += SaveGameList;
        
    }

    public GameList MainGameList { get; set; }

    private ObservableCollection<Game> _games = null!;
    public ObservableCollection<Game> Games
    {
        get => _games; 
        set => this.RaiseAndSetIfChanged(ref _games, value); //Saving to JSON could be done here
    }

    private Game _selectedGame = null!;
    public Game SelectedGame
    {
        get => _selectedGame;
        set => this.RaiseAndSetIfChanged(ref _selectedGame, value);
    }

    public ReactiveCommand<Unit, Unit> AddGameCommand { get; }
    public ReactiveCommand<Unit, Unit> EditGameCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveGameCommand { get; }
    public Interaction<GameConfigViewModel, Game> ShowConfigDialog { get; }

    private async Task AddGame()
    {
        GameConfigViewModel config = new GameConfigViewModel();
        Game? result = await ShowConfigDialog.Handle(config);
        if (result != null)
        {
            Games.Add(result);
        }
    }

    private void RemoveGame()
    {
        _games.Remove(_selectedGame);
    }

    private async Task EditGame()
    {
        // Here we want to start the config view but pass in the existing stuff :)
        GameConfigViewModel config = new GameConfigViewModel();
        config.GameName = SelectedGame.GameName;
        config.DeployPath = SelectedGame.DeployDirectory;
        config.ModsPath = SelectedGame.ModDirectory;
        Game? result = await ShowConfigDialog.Handle(config);
        if (result != null)
        {
            Games.Remove(SelectedGame);
            Games.Add(result);
        }
    }

    private void SaveGameList(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //We need to save the list of games that is displayed using a GameList object
        MainGameList.Games = new List<Game>(Games); 
        //For now this is a very simple (and unsafe) way to do this, but in future we could do it with iteration and the ModifyMod method in GameList
        MainGameList.SaveList();
    }

}