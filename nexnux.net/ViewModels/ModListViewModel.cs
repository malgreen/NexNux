using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nexnux.net.Models;
using Avalonia.Xaml.Interactions.DragAndDrop;
using ReactiveUI;
using System.Reactive;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;

namespace nexnux.net.ViewModels
{
    public class ModListViewModel : ViewModelBase
    {
        public ModListViewModel()
        {
            VisibleMods = new ObservableCollection<Mod>();
            VisibleMods.CollectionChanged += UpdateModList;

            InstallModCommand = ReactiveCommand.Create(InstallMod);
            UninstallModCommand = ReactiveCommand.Create(UninstallMod);

            this.WhenAnyValue(x => x.SelectedMod).Subscribe(x => UpdateModInfo());            
        }
        private int _testModIterator;

        private Game _currentGame;
        public Game CurrentGame
        {
            get => _currentGame;
            set => this.RaiseAndSetIfChanged(ref _currentGame, value);
        }

        private ModList _currentModList;
        public ModList CurrentModList
        {
            get => _currentModList;
            set => this.RaiseAndSetIfChanged(ref _currentModList, value);
        }

        private ObservableCollection<Mod> _visibleMods;
        public ObservableCollection<Mod> VisibleMods
        {
            get => _visibleMods;
            set => this.RaiseAndSetIfChanged(ref _visibleMods, value);
        }

        private Mod _selectedMod;
        public Mod SelectedMod
        {
            get => _selectedMod;
            set => this.RaiseAndSetIfChanged(ref _selectedMod, value);
        }

        private string _modInfo;
        public string ModInfo
        {
            get => _modInfo;
            set => this.RaiseAndSetIfChanged(ref _modInfo, value);
        }

        public ReactiveCommand<Unit, Unit> InstallModCommand { get; }
        public ReactiveCommand<Unit, Unit> UninstallModCommand { get; }


        public void UpdateCurrentGame(Game game)
        {
            VisibleMods.CollectionChanged -= UpdateModList; //Not doing this might lead to memory leak
            CurrentGame = game;
            string modListFile = CurrentGame.ModListFile;
            CurrentModList = new ModList(modListFile);

            VisibleMods = new ObservableCollection<Mod>(CurrentModList.LoadList());
            SetModListeners(VisibleMods, null);

            _testModIterator = VisibleMods.Count;
            VisibleMods.CollectionChanged += UpdateModList;
        }

        void InstallMod()
        {
            // For now this adds a placeholder mod
            _testModIterator++;
            string modName = $"Mod{_testModIterator}";
            string modPath = $"C:\\FakePath\\mod{_testModIterator}";
            Random rnd = new Random();
            double modSize = rnd.NextDouble();
            long modIndex = 0;
            bool modEnabled = false;
            Mod mod = new Mod(modName, modPath, modSize, modIndex, modEnabled);
            VisibleMods.Add(mod);
        }

        void UninstallMod()
        {
            if (SelectedMod == null) return;
            VisibleMods.Remove(SelectedMod);
        }

        void UpdateModInfo()
        {
            if (SelectedMod == null) return;

            string finalModInfo = string.Empty;
            finalModInfo += SelectedMod.ModName + "\n";
            finalModInfo += SelectedMod.ModPath + "\n";
            finalModInfo += SelectedMod.FileSize + " gb\n";
            finalModInfo += "Is enabled: " + SelectedMod.Enabled + "\n";
            //This can all be changed later, but the subscribtion to property changes works
            ModInfo = finalModInfo;
        }

        void UpdateModList(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(Mod mod in VisibleMods)
            {
                mod.Index = VisibleMods.IndexOf(mod);
            }
            SetModListeners(e.NewItems, e.OldItems);
            SaveVisibleList();
        }

        private void SetModListeners(IList newItems, IList oldItems)
        {
            if (newItems != null)
            {
                foreach(INotifyPropertyChanged mod in newItems)
                {
                    mod.PropertyChanged += Mod_PropertyChanged;
                }
            }
            if (oldItems != null)
            {
                foreach(INotifyPropertyChanged mod in oldItems)
                {
                    mod.PropertyChanged -= Mod_PropertyChanged;
                }
            }
        }

        private void Mod_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateModInfo();
            SaveVisibleList();
        }

        private void SaveVisibleList()
        {
            CurrentModList.Mods = VisibleMods.ToList();
            CurrentModList.SaveList();
        }
    }
}
