using System.Collections.ObjectModel;
using System.IO;

namespace NexNux.Models;

public class ModFileItem : IModItem
{
    public ModFileItem(string itemPath)
    {
        SubItems = new ObservableCollection<IModItem>();
        ItemPath = itemPath;
        ItemName = "🗋 " + Path.GetFileName(itemPath);
    }
    public ObservableCollection<IModItem> SubItems { get; set; }
    public string ItemPath { get; }
    public string ItemName { get; set; }
}