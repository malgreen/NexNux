using NexNux.Core.Models;
using NexNux.Core.Utilities.Serialization;

namespace NexNux.Core.Repositories;

public class ModRepositoryJson : IModRepository
{
    private readonly string _jsonPath;

    public ModRepositoryJson(Game game)
    {
        _jsonPath = Path.Combine(game.NexNuxDirectory, "mods.json");
    }

    public async Task<List<Mod>> GetMods()
    {
        return await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, ModsSerializerContext.Default.ListMod);
    }

    public async Task SetMods(List<Mod> mods)
    {
        await JsonListHelper.SerializeListToJsonAsync(mods, _jsonPath, ModsSerializerContext.Default.ListMod);
    }

    public async Task<Mod?> GetModById(Guid modId)
    {
        return (await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, ModsSerializerContext.Default.ListMod)).Find(m => m.Id == modId);
    }

    public async Task AddMod(Mod mod)
    {
        var mods = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, ModsSerializerContext.Default.ListMod);
        mods.Add(mod);
        await JsonListHelper.SerializeListToJsonAsync(mods, _jsonPath, ModsSerializerContext.Default.ListMod);
    }

    public async Task RemoveModById(Guid modId)
    {
        var mods = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, ModsSerializerContext.Default.ListMod);
        mods = mods.Where(m => m.Id != modId).ToList();
        await JsonListHelper.SerializeListToJsonAsync(mods, _jsonPath, ModsSerializerContext.Default.ListMod);
    }

    public async Task ModifyMod(Mod mod)
    {
        var mods = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, ModsSerializerContext.Default.ListMod);
        var index = mods.FindIndex(m => m.Id == mod.Id);
        if (index == -1)
            throw new Exception("Mod ID not found!");
        mods[index].Name = mod.Name;
        mods[index].Path = mod.Path;
        mods[index].IsEnabled = mod.IsEnabled;
        await JsonListHelper.SerializeListToJsonAsync(mods, _jsonPath, ModsSerializerContext.Default.ListMod);
    }

    public async Task ReorderModByIndices(int oldIndex, int newIndex)
    {
        var mods = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, ModsSerializerContext.Default.ListMod);
        var mod = mods[oldIndex];
        mods.RemoveAt(oldIndex);
        mods.Insert(newIndex, mod);
        await JsonListHelper.SerializeListToJsonAsync(mods, _jsonPath, ModsSerializerContext.Default.ListMod);
    }
}