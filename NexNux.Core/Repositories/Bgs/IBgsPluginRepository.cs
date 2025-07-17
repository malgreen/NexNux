using NexNux.Core.Models.Bgs;

namespace NexNux.Core.Repositories.Bgs;

public interface IBgsPluginRepository
{
    public Task<List<BgsPlugin>> GetBgsPlugins();
    public Task UpdateBgsPlugin(BgsPlugin bgsPlugin);
    public Task ReorderBgsPluginByIndices(int oldIndex, int newIndex);
}