using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Xaml.Interactions.DragAndDrop;
using ReactiveUI;
using System.Reactive;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.IO;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NexNux.Models;
using NexNux.Views;

namespace NexNux.ViewModels
{
    public class ModListViewModel : ViewModelBase
    {
        public ModListViewModel()
        {
            ShowModInstallDialog = new Interaction<ModConfigViewModel, Mod?>();
            ShowModUninstallDialog = new Interaction<Mod, bool>();
            ShowErrorDialog = new Interaction<string, bool>();
            ShowModExistsDialog = new Interaction<Mod, bool>();
            ShowGameList = new Interaction<Unit, Unit>();

            VisibleMods = new ObservableCollection<Mod>();
            VisibleMods.CollectionChanged += UpdateModList;

            InstallModCommand = ReactiveCommand.Create(InstallMod);
            UninstallModCommand = ReactiveCommand.Create(UninstallMod);
            ChangeGameCommand = ReactiveCommand.Create(ChangeGame);

            this.WhenAnyValue(x => x.SelectedMod).Subscribe(x => UpdateModInfo());            
        }

        private Game _currentGame = null!;
        public Game CurrentGame
        {
            get => _currentGame;
            set => this.RaiseAndSetIfChanged(ref _currentGame, value);
        }

        private ModList _currentModList = null!;
        public ModList CurrentModList
        {
            get => _currentModList;
            set => this.RaiseAndSetIfChanged(ref _currentModList, value);
        }

        private ObservableCollection<Mod> _visibleMods = null!;
        public ObservableCollection<Mod> VisibleMods
        {
            get => _visibleMods;
            set => this.RaiseAndSetIfChanged(ref _visibleMods, value);
        }

        private Mod _selectedMod = null!;
        public Mod SelectedMod
        {
            get => _selectedMod;
            set => this.RaiseAndSetIfChanged(ref _selectedMod, value);
        }

        private string _modInfo = null!;
        public string ModInfo
        {
            get => _modInfo;
            set => this.RaiseAndSetIfChanged(ref _modInfo, value);
        }

        public ReactiveCommand<Unit, Unit> InstallModCommand { get; }
        public ReactiveCommand<Unit, Unit> UninstallModCommand { get; }
        public ReactiveCommand<Unit, Unit> ChangeGameCommand { get; }
        public Interaction<ModConfigViewModel, Mod?> ShowModInstallDialog { get; }
        public Interaction<Mod, bool> ShowModUninstallDialog { get; }
        public Interaction<string, bool> ShowErrorDialog { get; }
        public Interaction<Mod, bool> ShowModExistsDialog { get; }
        public Interaction<Unit, Unit> ShowGameList { get; }


        public void UpdateCurrentGame(Game game)
        {
            VisibleMods.CollectionChanged -= UpdateModList; //Not doing this might lead to memory leak
            CurrentGame = game;
            string modListFile = CurrentGame.ModListFile;
            CurrentModList = new ModList(modListFile);

            VisibleMods = new ObservableCollection<Mod>(CurrentModList.LoadList());
            SetModListeners(VisibleMods, null!);

            VisibleMods.CollectionChanged += UpdateModList;
        }

        async void InstallMod()
        {
            try
            {
                string installCacheDir = Path.Combine(CurrentGame.ModSettingsDirectory, "__installcache");
                ModConfigViewModel modConfigViewModel = new ModConfigViewModel();
                modConfigViewModel.CurrentGame = CurrentGame;
                Mod mod = await ShowModInstallDialog.Handle(modConfigViewModel);
                if (mod == null)
                {
                    if (Directory.Exists(installCacheDir) && !modConfigViewModel.IsExtracting){ Directory.Delete(installCacheDir, true); }
                    return;
                }

                Mod existingMod = VisibleMods.FirstOrDefault(item => item.ModName == mod.ModName);
                string installedModPath = Path.Combine(CurrentGame.ModDirectory, mod.ModName);

                if (existingMod != null)
                {
                    bool result = await ShowModExistsDialog.Handle(mod);
                    if (result)
                    {
                        MoveExtractedFiles(modConfigViewModel.CurrentRoot.ItemPath, installedModPath);
                        DirectoryInfo dirInfo = new DirectoryInfo(existingMod.ModPath);
                        existingMod.FileSize = Math.Round(await Task.Run(() => dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length)) * 0.000001); //converts bytes to mb
                    }
                }
                else
                {
                    MoveExtractedFiles(modConfigViewModel.CurrentRoot.ItemPath, installedModPath);
                    DirectoryInfo dirInfo = new DirectoryInfo(mod.ModPath);
                    mod.FileSize = Math.Round(await Task.Run(() => dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length)) * 0.000001); //converts bytes to mb

                    VisibleMods.Add(mod);
                }
                if (Directory.Exists(installCacheDir) && !modConfigViewModel.IsExtracting){ Directory.Delete(installCacheDir, true); }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                await ShowErrorDialog.Handle(e.Message);
            }

        }

        async void UninstallMod()
        {
            if (SelectedMod == null) return;
            try
            {
                bool result = await ShowModUninstallDialog.Handle(SelectedMod);
                if (!result) return;
                SelectedMod.DeleteFiles();
                VisibleMods.Remove(SelectedMod);
            }
            catch (DirectoryNotFoundException)
            {
                VisibleMods.Remove(SelectedMod); //this exception doesn't matter
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                await ShowErrorDialog.Handle(e.Message);
            }
        }

        void UpdateModInfo()
        {
            if (SelectedMod == null)
            {
                ModInfo = "No mod selected";
            }
            else
            {
                string finalModInfo = string.Empty;
                finalModInfo += SelectedMod.ModName + "\n";
                finalModInfo += SelectedMod.ModPath + "\n";
                finalModInfo += SelectedMod.FileSize + " MB\n";
                finalModInfo += "Is enabled: " + SelectedMod.Enabled + "\n";
                //This can all be changed later, but the subscription to property changes works
                ModInfo = finalModInfo;
            }
        }

        void UpdateModList(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(Mod mod in VisibleMods)
            {
                if (mod == null) continue;
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
        private void MoveExtractedFiles(string source, string target)
        {
            Directory.CreateDirectory(target); // Without this, there is an exception when installing empty files
            // Taken from https://stackoverflow.com/a/2553245

            var sourcePath = source;
            var targetPath = target;
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
        }
        private async void ChangeGame()
        {
            try
            {
                await ShowGameList.Handle(Unit.Default);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
