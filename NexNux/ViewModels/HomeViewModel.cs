using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Material.Icons;
using NexNux.Models;
using NexNux.Models.Gamebryo;
using NexNux.Utilities;
using NexNux.Utilities.ModDeployment;
using NexNux.Views;
using ReactiveUI;

namespace NexNux.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        DeploymentTotal = 1;
        
        ShowErrorDialog = new Interaction<string, bool>();
        DeployModsCommand = ReactiveCommand.Create(DeployMods);
        ClearModsCommand = ReactiveCommand.Create(ClearMods);

        this.WhenAnyValue(x => x.IsDeployed).Subscribe(_ => UpdateDeploymentStatus());
        this.WhenAnyValue(x => x.IsDeploying).Subscribe(_ => UpdateDeploymentStatus());
    }

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

    private ModList _currentModList = null!;
    public ModList CurrentModList
    {
        get => _currentModList;
        set => this.RaiseAndSetIfChanged(ref _currentModList, value);
    }

    private GamebryoPluginList _currentPluginList = null!;
    public GamebryoPluginList CurrentPluginList
    {
        get => _currentPluginList;
        set => this.RaiseAndSetIfChanged(ref _currentPluginList, value);
    }

    private bool _isDeployed;
    public bool IsDeployed
    {
        get => _isDeployed;
        set => this.RaiseAndSetIfChanged(ref _isDeployed, value);
    }

    private bool _isDeploying;
    public bool IsDeploying
    {
        get => _isDeploying;
        set => this.RaiseAndSetIfChanged(ref _isDeploying, value);
    }

    private string _deploymentStatus = null!;
    public string DeploymentStatus
    {
        get => _deploymentStatus;
        set => this.RaiseAndSetIfChanged(ref _deploymentStatus, value);
    }

    private MaterialIconKind _deploymentStatusIcon;
    public MaterialIconKind DeploymentStatusIcon
    {
        get => _deploymentStatusIcon;
        set => this.RaiseAndSetIfChanged(ref _deploymentStatusIcon, value);
    }

    private double _deploymentProgress;
    public double DeploymentProgress
    {
        get => _deploymentProgress;
        set => this.RaiseAndSetIfChanged(ref _deploymentProgress, value);
    }

    private double _deploymentTotal;
    public double DeploymentTotal
    {
        get => _deploymentTotal;
        set => this.RaiseAndSetIfChanged(ref _deploymentTotal, value);
    }

    private ModListViewModel? _modListViewModel;
    private PluginListViewModel? _pluginListViewModel;
    
    public ReactiveCommand<Unit, Unit> DeployModsCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearModsCommand { get; }
    public Interaction<string, bool> ShowErrorDialog { get; }
    
    public void UpdateGame(Game game)
    {
        CurrentGame = game;
        CurrentTitle = "NexNux - " + game;
        TabItems = new ObservableCollection<NexNuxTabItem>();
        IsDeployed = CurrentGame.Settings.RecentlyDeployed;
        InitializeTabs();
    }

    public void InitializeTabs()
    {
        InitializeModsTab();
        if (CurrentGame?.Type != GameType.Generic)
        {
            InitializePluginsTab();
        }
        InitializeSettingsTab();
    }

    private void InitializeModsTab()
    {
        _modListViewModel = new ModListViewModel()
        {
            CurrentGame = CurrentGame
        };
        _modListViewModel.ModListChanged += ModListViewModel_OnModListChanged;
        CurrentModList = _modListViewModel.CurrentModList;
        ModListView modListView = new ModListView
        {
            DataContext = _modListViewModel
        };

        NexNuxTabItem modsTabItem = new NexNuxTabItem("Mods", MaterialIconKind.Plugin, modListView);
        TabItems.Add(modsTabItem);
    }

    private void ModListViewModel_OnModListChanged(object? sender, EventArgs e)
    {
        IsDeployed = false;
        if (sender is ModListViewModel mlvm)
        {
            CurrentModList = mlvm.CurrentModList;
        }
    }

    private void InitializePluginsTab()
    {
        if (CurrentGame == null || CurrentGame.AppDataDirectory == null) return;
        _pluginListViewModel = new PluginListViewModel()
        {
            CurrentGame = CurrentGame
        };
        CurrentPluginList = _pluginListViewModel.CurrentPluginList;
        PluginListView pluginListView = new PluginListView()
        {
            DataContext = _pluginListViewModel
        };
        
        NexNuxTabItem pluginsTabItem = new NexNuxTabItem("Plugins", MaterialIconKind.FormatListBulleted, pluginListView);
        TabItems.Add(pluginsTabItem);
    }

    private void InitializeSettingsTab()
    {
        NexNuxTabItem settingsTabItem = new NexNuxTabItem("Settings", MaterialIconKind.Settings, new UserControl());
        TabItems.Add(settingsTabItem);
    }
    
    private async void DeployMods()
    {
        if (CurrentGame == null) return;
        try
        {
            IsDeploying = true;
            DeploymentTotal = GetFileAmount(CurrentModList.GetActiveMods());
            
            IModDeployer modDeployer = new SymLinkDeployer(CurrentGame);
            modDeployer.FileDeployed += ModDeployer_FileDeployed;
            await Task.Run(() => modDeployer.Deploy(CurrentModList.GetActiveMods()));

            if (CurrentGame.Type != GameType.Generic && _pluginListViewModel != null)
            {
                await Task.Run(() => CurrentPluginList.RefreshFromDeployDirectory());
                _pluginListViewModel.UpdatePlugins(CurrentPluginList);
            }

            IsDeploying = false;
            IsDeployed = true;
            DeploymentProgress = 0;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
            IsDeployed = false;
            await ShowErrorDialog.Handle(e.Message);
        }
    }

    private void ModDeployer_FileDeployed(object? sender, FileDeployedArgs e)
    {
        DeploymentProgress = e.Progress;
    }
    
    private double GetFileAmount(List<Mod?> mods)
    {
        int amount = 0;
        foreach (Mod? mod in mods)
        {
            if (mod == null) continue;
            DirectoryInfo dir = new DirectoryInfo(mod.ModPath);
            foreach (FileInfo _ in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                amount++;
            }
        }
        return amount;
    }

    private async void ClearMods()
    {
        if (CurrentGame == null) return;
        try
        {
            IsDeploying = true;
            
            IModDeployer modDeployer = new SymLinkDeployer(CurrentGame);
            await Task.Run(() => modDeployer.Clear());
            
            if (CurrentGame.Type != GameType.Generic)
            {
                await Task.Run(() => CurrentPluginList.RefreshFromDeployDirectory());
            }
            
            IsDeployed = false;
            IsDeploying = false;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
            await ShowErrorDialog.Handle(e.Message);
        }
    }

    private void UpdateDeploymentStatus()
    {
        if (IsDeploying)
        {
            DeploymentStatusIcon = MaterialIconKind.HourglassEmpty;
            DeploymentStatus = "Deploying...";
        }
        else if (IsDeployed)
        {
            DeploymentStatusIcon = MaterialIconKind.Check;
            DeploymentStatus = "Mods deployed";
        }
        else
        {
            DeploymentStatusIcon = MaterialIconKind.Warning;
            DeploymentStatus = "Deployment needed";
        }
            
        if (CurrentGame == null) return;
        CurrentGame.Settings.RecentlyDeployed = IsDeployed;
        CurrentGame.Settings.Save();
        if (_pluginListViewModel != null)
        {
            _pluginListViewModel.BusyMessage = "Please deploy before managing plugins.";
            _pluginListViewModel.Busy = !IsDeployed;
        }
    }
}

