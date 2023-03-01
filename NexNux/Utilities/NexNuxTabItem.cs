using Avalonia.Controls;
using Material.Icons;

namespace NexNux.Utilities;

public class NexNuxTabItem
{
    public NexNuxTabItem(string header, MaterialIconKind icon, UserControl contentControl)
    {
        Header = header;
        Icon = icon;
        ContentControl = contentControl;
    }
    public string Header { get; }
    public MaterialIconKind Icon { get; }
    public UserControl ContentControl { get; }
}