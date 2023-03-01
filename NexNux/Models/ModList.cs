using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NexNux.Models;

public class ModList
{
    public ModList(string settingsDir)
    {
        _modListFileName = Path.Combine(settingsDir, "ModList.json");
        Mods = LoadList();
    }

    public List<Mod?> Mods { get; set; }
    private string _modListFileName;

    public void SaveList()
    {
        try
        {
            using FileStream createStream = File.Create(_modListFileName);
            JsonSerializer.Serialize(createStream, Mods, new JsonSerializerOptions(){ WriteIndented = true });
            createStream.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
            throw new UnauthorizedAccessException(ex.Message); // these nested exceptions are awful, pls fix (we also have try/catch in game config VM)
        }
    }

    public List<Mod?> LoadList()
    {
        List<Mod?> loadedMods = new List<Mod?>();

        if (!File.Exists(_modListFileName))
        {
            Mods = new List<Mod?>();
            SaveList();
        }
        else
        {
            string jsonString = File.ReadAllText(_modListFileName);
            loadedMods = JsonSerializer.Deserialize<List<Mod?>>(jsonString) ?? throw new InvalidOperationException();
        }

        return loadedMods;
    }

    public List<Mod?> GetActiveMods()
    {
        return new List<Mod?>(Mods.Where(d => d is {Enabled: true}));
    }

    public void InstallMod(Mod mod, string rootPath)
    {
        throw new NotImplementedException();
    }

    public void ModifyMod(string modName, string modPath, double fileSize, long index, bool enabled)
    {
        throw new NotImplementedException();
    }

    public void UninstallMod(Mod mod)
    {
        throw new NotImplementedException();
    }
}