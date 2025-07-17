using NexNux.Core.Models;

namespace NexNux.Core.Repositories;

public interface IModRepository
{
    public Task<List<Mod>> GetMods();
    public Task SetMods(List<Mod> mods);
    public Task<Mod?> GetModById(Guid modId);
    public Task AddMod(Mod mod);
    public Task RemoveModById(Guid modId);
    public Task ModifyMod(Mod mod);
    public Task ReorderModByIndices(int oldIndex, int newIndex);
}