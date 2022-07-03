using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace nexnux.net.Models;

public class Game
{
    public Game(string gameName, string deployDirectory, string modDirectory)
    {
        GameName = gameName;
        DeployDirectory = deployDirectory;
        ModDirectory = modDirectory;
        ValidateFolders();
        _modList = new ModList(ModListFile ?? throw new InvalidOperationException());
    }

    public string GameName { get; set; }
    public string DeployDirectory { get; set; }
    public string ModDirectory { get; set; }
    public string ModListFile { get; set; }
    private ModList _modList;

    void ValidateFolders()
    {
        Directory.CreateDirectory(DeployDirectory);
        Directory.CreateDirectory(ModDirectory);
        ModListFile = Path.Combine(ModDirectory, "ModList.json");
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