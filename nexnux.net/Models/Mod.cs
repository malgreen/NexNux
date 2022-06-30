namespace nexnux.net.Models;

public class Mod
{
    public Mod(string modName, double fileSize, long index, bool enabled)
    {
        ModName = modName;
        FileSize = fileSize;
        Index = index;
        Enabled = enabled;
    }

    public string ModName { get; }
    public double FileSize { get; }
    public long Index { get; set; }
    public bool  Enabled { get; set; }

    public override string ToString()
    {
        return ModName;
    }
}