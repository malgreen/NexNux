using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NexNux.App.Utilities;
using NexNux.App.Views;
using NexNux.Core.Models;
using NexNux.Core.Repositories;

namespace NexNux.App.ViewModels;

public partial class GameSelectionViewModel : ViewModelBase
{
    // private readonly GameService _gameService = new();
    private readonly IGameRepository _gameRepository = new GameRepositoryJson();
    
    [ObservableProperty] private IEnumerable<Game> _games = new List<Game>();
    [ObservableProperty] private Game? _selectedGame;
    
    
    

    
    public GameSelectionViewModel()
    {
        GetGames();
    }

    [RelayCommand]
    private async void AddGame()
    {
        var viewmodel = new GameConfigurationViewModel();
        var dialog = new GameConfigurationWindow();
        dialog.DataContext = viewmodel;

        // doesn't seem very mvvm to me
        var result = await DialogHelper.ShowDialog<Game?>(dialog);

        Console.WriteLine(result);
    }

    [RelayCommand]
    private void RemoveGame()
    {
        Console.WriteLine("removing game");
    }

    [RelayCommand]
    private void ChooseGame()
    {
        if (SelectedGame == null) return;
        Console.WriteLine($"choosing game: {SelectedGame.Name}");
    }

    private async void GetGames()
    {
        try
        {
            Games = await _gameRepository.GetGames();
        }
        catch (Exception ex)
        {
            // this doesn't work?
            DialogHelper.ShowMessageDialog("error", ex.Message);
        }
    }
}