using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NexNux.Models;

public class Game
{
    public Game(string gameName, string deployDirectory, string modDirectory)
    {
        GameName = gameName;
        DeployDirectory = deployDirectory;
        ModDirectory = modDirectory;
        ValidateInfo();
        _modList = new ModList(ModListFile ?? throw new InvalidOperationException());
    }

    public string GameName { get; set; }
    public string DeployDirectory { get; set; }
    public string ModDirectory { get; set; }
    public string ModListFile { get; set; }
    private ModList _modList;

    void ValidateInfo()
    {
        if (GameName.Equals(string.Empty))
            throw new Exception("Game must have a name");
        Directory.CreateDirectory(DeployDirectory);
        Directory.CreateDirectory(ModDirectory);
        ModListFile = Path.Combine(ModDirectory, "ModList.json");

        FileInfo deployInfo = new FileInfo(DeployDirectory);
        FileInfo modsInfo = new FileInfo(ModDirectory);
        if (!Equals(Path.GetPathRoot(deployInfo.FullName), Path.GetPathRoot(modsInfo.FullName)))
            throw new Exception("Directories must reside on the same drive"); // Hardlink deployment cannot be done if different drives
    }

    public List<Mod> GetAllMods()
    {
        return _modList.Mods;
    }

    public List<Mod> GetActiveMods()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return GameName;
    }
}