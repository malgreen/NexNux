using System.Collections.ObjectModel;
using Material.Icons;
using Material.Icons.Avalonia;
using NexNux.Models;
using NexNux.Utilities;
using NexNux.Views;
using ReactiveUI;

namespace NexNux.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private Game? _currentGame;
    public Game? CurrentGame
    {
        get => _currentGame;
        set => this.RaiseAndSetIfChanged(ref _currentGame, value);
    }

    private string _currentTitle = null!;
    public string CurrentTitle
    {
        get => _currentTitle;
        set => this.RaiseAndSetIfChanged(ref _currentTitle, value);
    }

    private ObservableCollection<NexNuxTabItem> _tabItems = null!;
    public ObservableCollection<NexNuxTabItem> TabItems
    {
        get => _tabItems;
        set => this.RaiseAndSetIfChanged(ref _tabItems, value);
    }
    public void UpdateGame(Game game)
    {
        CurrentGame = game;
        CurrentTitle = "NexNux - " + game;
        TabItems = new ObservableCollection<NexNuxTabItem>();
        InitializeTabs();
    }

    public void InitializeTabs()
    {
        InitializeModsTab();
    }

    private void InitializeModsTab()
    {
        ModListViewModel modListViewModel = new ModListViewModel()
        {
            CurrentGame = CurrentGame
        };
        ModListView modListView = new ModListView
        {
            DataContext = modListViewModel
        };
        MaterialIcon icon = new MaterialIcon()
        {
            Kind = MaterialIconKind.About
        };
        
        NexNuxTabItem modsTabItem = new NexNuxTabItem("Mods", icon, modListView);
        TabItems.Add(modsTabItem);
    }
}

