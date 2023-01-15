using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexNux.Models;

public class GameSettings
{
    public GameSettings(string settingsDir)
    {
        _settingsFileName = Path.Combine(settingsDir, "Settings.json");
        Load();
    }
    [JsonConstructor]
    public GameSettings() { }

    private bool _recentlyDeployed;
    public bool RecentlyDeployed
    {
        get => _recentlyDeployed;
        set
        {
            _recentlyDeployed = value;
            // Saving on property change should be possible, but not always necessary.
            // It also creates complications when loading.
            //Save();
        }
    }

    private string? _settingsFileName;

    public void Load()
    {
        if (!File.Exists(_settingsFileName))
        {
            Initialize();
        }
        else
        {
            string jsonString = File.ReadAllText(_settingsFileName);
            GameSettings? loadedSettings = JsonSerializer.Deserialize<GameSettings>(jsonString);
            if (loadedSettings == null) return;
            
            // Below all properties should be set to the loaded objects fields
            RecentlyDeployed = loadedSettings.RecentlyDeployed;
        }
    }

    public void Save()
    {
        if (_settingsFileName == null) return;
        using FileStream createStream = File.Create(_settingsFileName);
        JsonSerializer.Serialize(createStream, this, new JsonSerializerOptions() { WriteIndented = true });
        createStream.Dispose();
    }

    /// <summary>
    /// Initializes all properties of the settings to default values, and serializes the object.
    /// </summary>
    private void Initialize()
    {
        RecentlyDeployed = false;
        Save();
    }
}
