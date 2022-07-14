using System.ComponentModel;

namespace nexnux.net.Models;

public class Mod : INotifyPropertyChanged
{
    public Mod(string modName, string modPath, double fileSize, long index, bool enabled)
    {
        ModName = modName;
        ModPath = modPath;
        FileSize = fileSize;
        Index = index;
        Enabled = enabled;
    }

    public string ModName { get; }
    public string ModPath { get; set; }
    public double FileSize { get; }

    private long _index;
    public long Index
    {
        get => _index;
        set
        {
            _index = value;
            NotifyPropertyChanged();
        }
    }

    private bool _enabled;
    public bool  Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            NotifyPropertyChanged();
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

    public override string ToString()
    {
        return ModName;
    }
}