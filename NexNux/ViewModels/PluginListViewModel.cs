using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using NexNux.Models;
using NexNux.Models.Gamebryo;
using ReactiveUI;

namespace NexNux.ViewModels;

public class PluginListViewModel : ViewModelBase
{
    public PluginListViewModel()
    {
        Busy = false;
        BusyMessage = "";
        ShowErrorDialog = new Interaction<string, bool>();
        this.WhenAnyValue(x => x.CurrentGame).Subscribe(_ => UpdateCurrentGame());
    }

    private Game? _currentGame;
    public Game? CurrentGame
    {
        get => _currentGame;
        set => this.RaiseAndSetIfChanged(ref _currentGame, value);
    }

    private GamebryoPluginList _currentPluginList = null!;
    public GamebryoPluginList CurrentPluginList
    {
        get => _currentPluginList;
        set => this.RaiseAndSetIfChanged(ref _currentPluginList, value);
    }

    private ObservableCollection<GamebryoPlugin> _visiblePlugins;
    public ObservableCollection<GamebryoPlugin> VisiblePlugins
    {
        get => _visiblePlugins;
        set => this.RaiseAndSetIfChanged(ref _visiblePlugins, value);
    }

    private GamebryoPlugin _selectedPlugin;
    public GamebryoPlugin SelectedPlugin
    {
        get => _selectedPlugin;
        set => this.RaiseAndSetIfChanged(ref _selectedPlugin, value);
    }

    private bool _busy;
    public bool Busy
    {
        get => _busy;
        set => this.RaiseAndSetIfChanged(ref _busy, value);
    }

    private string _busyMessage;
    public string BusyMessage
    {
        get => _busyMessage;
        set => this.RaiseAndSetIfChanged(ref _busyMessage, value);
    }
    
    public Interaction<string, bool> ShowErrorDialog { get; }
    public event EventHandler<EventArgs>? PluginListChanged;
    
    private void UpdateCurrentGame()
    {
        if (CurrentGame == null || CurrentGame.AppDataDirectory == null) return;
        CurrentPluginList = new GamebryoPluginList(
            CurrentGame.DeployDirectory,
            CurrentGame.SettingsDirectory,
            CurrentGame.AppDataDirectory, 
            CurrentGame.Type
            );
        ObservableCollection<GamebryoPlugin> prevPlugins = VisiblePlugins;
        VisiblePlugins = CurrentPluginList.Plugins;
        SetPluginListeners(VisiblePlugins, prevPlugins);
    }

    public async void ReorderPlugin(int oldIndex, int newIndex)
    {
        Busy = true;
        VisiblePlugins.Move(oldIndex, newIndex);
        CurrentPluginList.Plugins = VisiblePlugins;
        await Task.Run(() => CurrentPluginList.Synchronize());
        // PluginListChanged?.Invoke(this, e);
        Busy = false;
    }
    
    private void SetPluginListeners(IList? newItems, IList? oldItems)
    {
        if (newItems != null)
        {
            foreach(INotifyPropertyChanged plugin in newItems)
            {
                plugin.PropertyChanged += Plugin_PropertyChanged;
            }
        }
        if (oldItems != null)
        {
            foreach(INotifyPropertyChanged plugin in oldItems)
            {
                plugin.PropertyChanged -= Plugin_PropertyChanged;
            }
        }
    }

    private async void Plugin_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // This should only handled the enabled status, as it should only be run once to avoid writing the file 
        // at the same time
        if (e.PropertyName != "Enabled") return;
        BusyMessage = "Saving...";
        Busy = true;
        CurrentPluginList.Plugins = VisiblePlugins;
        await Task.Run(() => CurrentPluginList.Synchronize());
        PluginListChanged?.Invoke(this, e);
        Busy = false;
    }
}