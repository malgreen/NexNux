using System.ComponentModel;

namespace NexNux.Models.Gamebryo;

public class GamebryoPlugin : INotifyPropertyChanged
{
    public GamebryoPlugin(string pluginName, GamebryoPluginType pluginType, long loadOrderIndex, bool isEnabled)
    {
        PluginName = pluginName;
        PluginType = pluginType;
        LoadOrderIndex = loadOrderIndex;
        IsEnabled = isEnabled;
    }
    
    public string PluginName { get; }
    public GamebryoPluginType PluginType { get; }

    private long _loadOrderIndex;
    public long LoadOrderIndex
    {
        get => _loadOrderIndex;
        set
        {
            _loadOrderIndex = value;
            NotifyPropertyChanged("Index");
        }
    }

    private bool _isEnabled;
    public bool  IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            NotifyPropertyChanged("Enabled");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is GamebryoPlugin plugin && obj != null)
        {
            return PluginName == plugin.PluginName;
        }
        else
        {
            return false;
        }
    }
    public override int GetHashCode()
    {
        return PluginName.GetHashCode();
    }

    public override string ToString()
    {
        return PluginName;
    }
}