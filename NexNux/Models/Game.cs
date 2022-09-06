using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NexNux.Models;

public class Game
{
    public Game(string gameName, string deployDirectory, string modDirectory)
    {
        GameName = gameName;
        DeployDirectory = deployDirectory;
        ModDirectory = modDirectory;
        ModSettingsDirectory = Path.Combine(ModDirectory, ".NexNux");
        ValidateInfo();
        _modList = new ModList(ModListFile ?? throw new InvalidOperationException());
    }

    public string GameName { get; set; }
    public string DeployDirectory { get; set; }
    public string ModDirectory { get; set; }
    public string ModSettingsDirectory { get; set; }
    public string ModListFile { get; set; }
    private ModList _modList;

    void ValidateInfo()
    {
        if (GameName.Equals(string.Empty))
            throw new Exception("Game must have a name");

        FileInfo deployInfo = new FileInfo(DeployDirectory);
        FileInfo modsInfo = new FileInfo(ModDirectory);
        if (!Equals(Path.GetPathRoot(deployInfo.FullName), Path.GetPathRoot(modsInfo.FullName)))
            throw new Exception("Directories must reside on the same drive"); // Hardlink deployment cannot be done if different drives

        Directory.CreateDirectory(DeployDirectory);
        Directory.CreateDirectory(ModDirectory);
        Directory.CreateDirectory(ModSettingsDirectory);
        ModListFile = Path.Combine(ModSettingsDirectory, "ModList.json");
    }

    public List<Mod> GetAllMods()
    {
        return _modList.Mods;
    }

    public List<Mod> GetActiveMods()
    {
        return new List<Mod>(_modList.Mods.Where(d => d.Enabled));
    }

    /// <summary>
    /// Deletes all files and folders from the Game's 'mods' directory
    /// </summary>
    /// TODO: When mod deployment is implemented, should also restore all original files first
    public void DeleteMods()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(ModDirectory);

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