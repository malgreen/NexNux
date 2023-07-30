using NexNux.Models;
using NexNux.Models.Gamebryo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NexNux.Utilities;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Game>))]
internal partial class GamesSerializerContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Mod>))]
internal partial class ModsSerializerContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ObservableCollection<GamebryoPlugin>))]
internal partial class GbPluginsSerializerContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GameSettings))]
internal partial class GameSettingsSerializerContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<string>))]
internal partial class StringsSerializerContext : JsonSerializerContext
{
}
