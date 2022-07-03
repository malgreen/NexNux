using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace nexnux.net.Models;

public class ModList
{
    public ModList(string modListFileName)
    {
        _modListFileName = modListFileName;
        Mods = LoadList();
    }

    public List<Mod> Mods { get; set; }
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
        }
    }

    public List<Mod> LoadList()
    {
        List<Mod> loadedMods = new List<Mod>();
        try
        {
            string jsonString = File.ReadAllText(_modListFileName);
            loadedMods = JsonSerializer.Deserialize<List<Mod>>(jsonString) ??
                         throw new InvalidOperationException();
        }
        catch (FileNotFoundException ex)
        {
            Debug.WriteLine(ex);
            Mods = new List<Mod>();
            SaveList();
            LoadList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return loadedMods;
    }

    void SetupFile()
    {
        try
        {
            using FileStream createStream = File.Create(_modListFileName);
            createStream.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }
    }

    public void ModifyMod(string modName, double fileSize, long index, bool enabled)
    {
        Mod mod = new Mod(modName, fileSize, index, enabled);
        RemoveMod(mod);
        Mods.Add(mod);
    }

    public void RemoveMod(Mod mod)
    {
        Mod? existingMod = Mods.Find(item => item.ModName == mod.ModName);
        while (existingMod != null)
        {
            Mods.Remove(existingMod);
            existingMod = Mods.Find(item => item.ModName == mod.ModName);
        }
    }
}