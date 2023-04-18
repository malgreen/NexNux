using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NexNux.Models.Gamebryo;

public class GamebryoPluginList
{
    public GamebryoPluginList(string deployDir, string settingsDir, string appDataDir, GameType gameType)
    {
        _pluginTypeDictionary = new Dictionary<string, GamebryoPluginType>
        {
            {".esl", GamebryoPluginType.ESL},
            {".esp", GamebryoPluginType.ESP},
            {".esm", GamebryoPluginType.ESM}
        };

        _deployDirPath = deployDir;
        _pluginListFileName = Path.Combine(settingsDir, "PluginList.json");
        _pluginsTxtPath = Path.Combine(appDataDir, "plugins.txt");
        _loadorderTxtPath = Path.Combine(appDataDir, "loadorder.txt");
        _gameType = gameType;
        Plugins = new ObservableCollection<GamebryoPlugin>();
        Load();
    }
    
    public ObservableCollection<GamebryoPlugin> Plugins { get; set; }
    private GameType _gameType;
    private Dictionary<string, GamebryoPluginType> _pluginTypeDictionary;
    private string _pluginListFileName;
    private string _pluginsTxtPath;
    private string _loadorderTxtPath;
    private string _deployDirPath;

    public void Save() 
    {
        using FileStream createStream = File.Create(_pluginListFileName);
        JsonSerializer.Serialize(createStream, Plugins, new JsonSerializerOptions(){ WriteIndented = true });
        createStream.Dispose();
        SetIndices();
        SetPluginTimeStamps();
        SavePluginsToFile(_pluginsTxtPath, _gameType == GameType.BGSPostSkyrim);
        SavePluginsToFile(_loadorderTxtPath, false);
    }

    public void Load()
    {
        if (!File.Exists(_pluginsTxtPath))
        {
            File.Create(_pluginsTxtPath);
        }

        if (!File.Exists(_loadorderTxtPath))
        {
            File.Create(_loadorderTxtPath);
        }

        try
        {
            string jsonString = File.ReadAllText(_pluginListFileName);
            Plugins = JsonSerializer.Deserialize<ObservableCollection<GamebryoPlugin>>(jsonString) ??
                      throw new InvalidOperationException();
        }
        catch (FileNotFoundException)
        {
            Plugins = new ObservableCollection<GamebryoPlugin>(GetPluginsFromFile(_pluginsTxtPath));
        }
        catch (JsonException)
        {
            File.Delete(_pluginListFileName);
        }

        Refresh();
        Synchronize();
    }

    public void Refresh()
    {
        List<GamebryoPlugin> pluginsFilePlugins = GetPluginsFromFile(_pluginsTxtPath);
        List<GamebryoPlugin> enabledPlugins = Plugins.Where(plugin => plugin.IsEnabled).ToList();
        foreach(var plugin in enabledPlugins)
        {
            int pluginIndexInFile = pluginsFilePlugins.FindIndex(p => p.Equals(plugin));
            int pluginIndexInEnabledList = enabledPlugins.IndexOf(plugin);
            int pluginIndexInFullList = Plugins.IndexOf(plugin);
            int indexDifference = pluginIndexInFullList - pluginIndexInEnabledList;

            Plugins.Move(pluginIndexInFullList, pluginIndexInFile + indexDifference);
            enabledPlugins = Plugins.Where(plugin => plugin.IsEnabled).ToList();
        }
        Save();
    }

    public void Synchronize()
    {
        List<GamebryoPlugin> pluginsFilePlugins = GetPluginsFromFile(_pluginsTxtPath);
        List<GamebryoPlugin> deployDirPlugins = GetPluginsFromDirectory(_deployDirPath);

        // Check plugins that are present in the file but not in the deploy directory
        // Remove these plugins
        List<GamebryoPlugin> absentPlugins = pluginsFilePlugins.Except(deployDirPlugins).ToList();
        Plugins = new ObservableCollection<GamebryoPlugin>(Plugins.Except(absentPlugins).ToList());

        // Get new plugins that are present in the deploy directory, but not in the file
        // These plugins can just be added to the list
        List<GamebryoPlugin> newPlugins = deployDirPlugins.Except(pluginsFilePlugins).ToList();
        Plugins = new ObservableCollection<GamebryoPlugin>(Plugins.Union(newPlugins).ToList());
        Save();
    }

    private void SavePluginsToFile(string filePath, bool asterixPrefix)
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath))
        {
            foreach(GamebryoPlugin plugin in Plugins)
            {
                if (!plugin.IsEnabled) continue;
                string pluginName = asterixPrefix ? string.Join("*", plugin.PluginName) : plugin.PluginName;
                streamWriter.WriteLine(pluginName);
            }
        }
    }

    private List<GamebryoPlugin> GetPluginsFromFile(string filePath)
    {
        List<GamebryoPlugin> readPlugins = new List<GamebryoPlugin>();
        foreach (string pluginLine in File.ReadLines(_pluginsTxtPath))
        {
            if (!_pluginTypeDictionary.TryGetValue(Path.GetExtension(pluginLine.ToLower()), out var pluginType)) continue;

            string pluginName = _gameType == GameType.BGS ? pluginLine : pluginLine.Substring(1);

            GamebryoPlugin plugin = new GamebryoPlugin(pluginName, pluginType, readPlugins.Count, true);
            readPlugins.Add(plugin);
        }
        return readPlugins;
    }

    private List<GamebryoPlugin> GetPluginsFromDirectory(string dirPath)
    {
        List<GamebryoPlugin> readPlugins = new List<GamebryoPlugin>();
        foreach (string file in Directory.GetFiles(dirPath))
        {
            if (!_pluginTypeDictionary.TryGetValue(Path.GetExtension(file.ToLower()), out var pluginType)) continue;
            GamebryoPlugin plugin = new GamebryoPlugin(Path.GetFileName(file), pluginType, readPlugins.Count, true);
            readPlugins.Add(plugin);
        }
        return readPlugins;
    }

    private void SetIndices()
    {
        foreach (GamebryoPlugin plugin in Plugins)
        {
            plugin.LoadOrderIndex = Plugins.IndexOf(plugin);
        }
    }

    private void SetPluginTimeStamps()
    {
        foreach (GamebryoPlugin plugin in Plugins)
        {
            string pluginPath = Path.Combine(_deployDirPath, plugin.PluginName);
            File.SetLastWriteTime(pluginPath, DateTime.Now);
        }
    }
}