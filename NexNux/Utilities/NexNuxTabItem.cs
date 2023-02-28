using Avalonia.Controls;
using Material.Icons.Avalonia;

namespace NexNux.Utilities;

public class NexNuxTabItem
{
    public NexNuxTabItem(string header, MaterialIcon icon, UserControl contentControl)
    {
        Header = header;
        Icon = icon;
        ContentControl = contentControl;
    }
    public string Header { get; }
    public MaterialIcon Icon { get; }
    public UserControl ContentControl { get; }
}