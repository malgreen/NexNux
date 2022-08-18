using Avalonia;
using NexNux.Models;
using ReactiveUI;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using NexNux.Utilities;

namespace NexNux.ViewModels;

public class ModConfigViewModel : ViewModelBase
{
    public ModConfigViewModel()
    {
        ExtractedFiles = new ObservableCollection<IModItem>();
        SetSelectionToRootCommand = ReactiveCommand.Create(SetSelectionToRoot);
        SetSelectionToClipboardCommand = ReactiveCommand.CreateFromTask(SetSelectionToClipboard);
        InstallModCommand = ReactiveCommand.CreateFromTask(InstallMod);
        CancelCommand = ReactiveCommand.CreateFromTask(Cancel);
    }

    private Game _currentGame;
    public Game CurrentGame
    {
        get => _currentGame;
        set => this.RaiseAndSetIfChanged(ref _currentGame, value);
    }

    private string _modName;
    public string ModName
    {
        get => _modName;
        set => this.RaiseAndSetIfChanged(ref _modName, value);
    }

    private string _modArchivePath;
    public string ModArchivePath
    {
        get => _modArchivePath;
        set => this.RaiseAndSetIfChanged(ref _modArchivePath, value);
    }

    private decimal _extractionProgress;
    public decimal ExtractionProgress
    {
        get => _extractionProgress;
        set => this.RaiseAndSetIfChanged(ref _extractionProgress, value);
    }

    private bool _isExtracting;
    public bool IsExtracting
    {
        get => _isExtracting;
        set => this.RaiseAndSetIfChanged(ref _isExtracting, value);
    }

    private ObservableCollection<IModItem> _extractedFiles;
    public ObservableCollection<IModItem> ExtractedFiles
    {
        get => _extractedFiles;
        set => this.RaiseAndSetIfChanged(ref _extractedFiles, value);
    }

    private IModItem _selectedItem;
    public IModItem SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    private long _archiveSize;
    public long ArchiveSize
    {
        get => _archiveSize;
        set => this.RaiseAndSetIfChanged(ref _archiveSize, value);
    }

    private ModFolderItem _currentRoot;
    public ModFolderItem CurrentRoot
    {
        get => _currentRoot;
        set => this.RaiseAndSetIfChanged(ref _currentRoot, value);
    }

    public ReactiveCommand<Unit, Unit> SetSelectionToRootCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SetSelectionToClipboardCommand { get; set; }
    public ReactiveCommand<Unit, Mod?> InstallModCommand { get; }
    public ReactiveCommand<Unit, Mod?> CancelCommand { get; }


    public async void UpdateModArchive(string archivePath)
    {
        if (CurrentGame == null) return;
        ModArchivePath = archivePath;
        ModName = Path.GetFileNameWithoutExtension(archivePath);
        string extractionPath = Path.Join(CurrentGame.ModDirectory, "__installcache");
        await ExtractArchiveAsync(ModArchivePath, extractionPath);
        await UpdateExtractedFiles(extractionPath);
    }

    private async Task ExtractArchiveAsync(string inputPath, string outputPath)
    {
        if (Directory.Exists(outputPath))
            Directory.Delete(outputPath, true);
        Directory.CreateDirectory(outputPath);
        IsExtracting = true;
        ExtractionProgress = 0;
        // TODO: Currently extraction progress is just number of entries in the archive, and is updated with 1 each time an entry is extracted
        // TODO: Could be better with bytes, for now it works because of the progressbar Maximum property is being set to the no. of entries

        try
        {
            await using Stream stream = File.OpenRead(inputPath);
            using IArchive archive = ArchiveFactory.Open(stream); //We have to use archive->reader, because ReaderFactory does not support Rar archives
            ArchiveSize = archive.Entries.Count(d => !d.IsDirectory); //Used in view
            using IReader? reader = archive.ExtractAllEntries();

            while (reader.MoveToNextEntry())
            {
                await Task.Run(() =>
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        ExtractionProgress++;
                        reader.WriteEntryToDirectory(outputPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }

        if (ExtractionProgress != 100) ExtractionProgress = 100;
        IsExtracting = false;
    }

    private async Task UpdateExtractedFiles(string rootPath)
    {
        ExtractedFiles = new ObservableCollection<IModItem>();
        ModFolderItem rootItem = new ModFolderItem(rootPath);
        CurrentRoot = rootItem;
        rootItem.ItemName = "root";
        rootItem.SubItems = GetSubItems(rootPath);
        //ExtractedFiles.Add(rootItem);
        ExtractedFiles = rootItem.SubItems; //TBD - should it also show the root folder, or just files within it?
    }

    private ObservableCollection<IModItem> GetSubItems(string itemPath)
    {
        ObservableCollection<IModItem> subItems = new ObservableCollection<IModItem>();
        string[] subFiles = Directory.GetFileSystemEntries(itemPath, "*", SearchOption.TopDirectoryOnly);

        foreach (string subFile in subFiles)
        {
            if (Directory.Exists(subFile)) // If it is a directory
            {
                ModFolderItem currentItem = new ModFolderItem(subFile);
                currentItem.SubItems = GetSubItems(subFile);
                subItems.Add(currentItem);
            }
            else
            {
                ModFileItem currentItem = new ModFileItem(subFile);
                subItems.Add(currentItem);
            }
        }
        return subItems;
    }

    private async void SetSelectionToRoot()
    {
        if (SelectedItem is ModFolderItem)
        {
            await UpdateExtractedFiles(SelectedItem.ItemPath);
        }
    }

    private async Task SetSelectionToClipboard()
    {
        try
        {
            string selectedPath = Path.GetDirectoryName(SelectedItem.ItemPath);
            await Application.Current.Clipboard.SetTextAsync(selectedPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }
    }

    public async Task<Mod?> InstallMod()
    {
        try
        {
            string installedModPath = Path.Combine(CurrentGame.ModDirectory, ModName);
            Mod mod = new Mod(ModName, installedModPath, 0, CurrentGame.GetAllMods().Count, false); //FileSize is updated in the ModListVM
            return mod;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
            return null;
        }
    }

    public async Task<Mod?> Cancel()
    {
        return null; //There is null-checking in the modlistviewmodel
    }
}