using System;
using System.ComponentModel;
using System.IO;

namespace NexNux.Models;

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

    private double _fileSize;
    public double FileSize
    {
        get => _fileSize;
        set
        {
            _fileSize = value;
            NotifyPropertyChanged("FileSize");
        }
    }

    private long _index;
    public long Index
    {
        get => _index;
        set
        {
            _index = value;
            NotifyPropertyChanged("Index");
        }
    }

    private bool _enabled;
    public bool  Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
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

    public void DeleteFiles()
    {
        Directory.Delete(ModPath, true);
    }

    public override string ToString()
    {
        return ModName;
    }
}