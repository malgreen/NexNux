using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace NexNux.Models;

public class Game
{
    public Game(string gameName, string deployDirectory, string modsDirectory)
    {
        GameName = gameName;
        DeployDirectory = deployDirectory;
        ModsDirectory = modsDirectory;
        SettingsDirectory = Path.Combine(ModsDirectory, ".NexNux");
        ValidateInfo();

        Settings = new GameSettings(SettingsDirectory ?? throw new InvalidOperationException());
        _modList = new ModList(SettingsDirectory ?? throw new InvalidOperationException());
    }

    public string GameName { get; set; }
    public string DeployDirectory { get; set; }
    public string ModsDirectory { get; set; }
    public string SettingsDirectory { get; set; }

    [JsonIgnore]
    public GameSettings Settings { get; private set; }
    private ModList _modList;

    void ValidateInfo()
    {
        if (GameName.Equals(string.Empty))
            throw new Exception("Game must have a name");

        FileInfo deployInfo = new FileInfo(DeployDirectory);
        FileInfo modsInfo = new FileInfo(ModsDirectory);
        if (!Equals(Path.GetPathRoot(deployInfo.FullName), Path.GetPathRoot(modsInfo.FullName)))
            throw new Exception("Directories must reside on the same drive"); // Hardlink deployment cannot be done if different drives

        Directory.CreateDirectory(DeployDirectory);
        Directory.CreateDirectory(ModsDirectory);
        Directory.CreateDirectory(SettingsDirectory);
    }

    public List<Mod?> GetAllMods()
    {
        return _modList.Mods;
    }

    public List<Mod?> GetActiveMods()
    {
        return new List<Mod?>(_modList.Mods.Where(d => d is {Enabled: true}));
    }

    /// <summary>
    /// Deletes all files and folders from the Game's 'mods' directory
    /// </summary>
    /// TODO: When mod deployment is implemented, should also restore all original files first
    public void DeleteMods()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(ModsDirectory);

        foreach (FileInfo file in dirInfo.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo dir in dirInfo.GetDirectories())
        {
            dir.Delete(true);
        }
    }

    public override string ToString()
    {
        return GameName;
    }
}