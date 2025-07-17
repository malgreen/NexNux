using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NexNux.Core.Utilities.Serialization;

public static class JsonListHelper
{
    public static async Task SerializeListToJsonAsync<T>(List<T> list, string jsonPath, JsonTypeInfo<List<T>> jsonTypeInfo)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath) ?? throw new InvalidOperationException());
        using var fileStream = File.Create(jsonPath);
        await JsonSerializer.SerializeAsync(fileStream, list, jsonTypeInfo);
        fileStream.Dispose();
    }

    public static async Task<List<T>> DeserializeJsonToListAsync<T>(string jsonPath, JsonTypeInfo<List<T>> jsonTypeInfo)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath) ?? throw new InvalidOperationException());
        using var textStream = File.OpenRead(jsonPath);
        var items = await JsonSerializer.DeserializeAsync(textStream, jsonTypeInfo) ?? new List<T>();
        textStream.Dispose();
        return items;
    }
}