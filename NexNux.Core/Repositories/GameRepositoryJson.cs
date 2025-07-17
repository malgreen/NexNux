using NexNux.Core.Models;
using NexNux.Core.Models.Bgs;
using NexNux.Core.Utilities.Serialization;

namespace NexNux.Core.Repositories;

public class GameRepositoryJson : IGameRepository
{
    private readonly string _jsonPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NexNux", "games.json");

    public async Task<List<Game>> GetGames()
    {
        return await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, GamesSerializerContext.Default.ListGame);
    }

    public async Task<Game?> GetGameById(Guid gameId)
    {
        var games = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, GamesSerializerContext.Default.ListGame);
        return games.Find(g => g.Id == gameId);
    }

    public async Task AddGame(Game game)
    {
        var games = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, GamesSerializerContext.Default.ListGame);
        games.Add(game);
        await JsonListHelper.SerializeListToJsonAsync(games, _jsonPath, GamesSerializerContext.Default.ListGame);
    }

    public async Task RemoveGameById(Guid gameId)
    {
        var games = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, GamesSerializerContext.Default.ListGame);
        games = games.Where(g => g.Id != gameId).ToList();
        await JsonListHelper.SerializeListToJsonAsync(games, _jsonPath, GamesSerializerContext.Default.ListGame);
    }

    public async Task ModifyGame(Game game)
    {
        var games = await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, GamesSerializerContext.Default.ListGame);
        var index = games.FindIndex(g => g.Id == game.Id);
        if (index == -1)
            throw new Exception("Game ID not found!");
        games[index].Name = game.Name;
        games[index].GameDirectory = game.GameDirectory;
        games[index].NexNuxDirectory = game.NexNuxDirectory;
        if (game is BgsGame bgsGame && games[index] is BgsGame)
            ((BgsGame)games[index]).AppDataDirectory = bgsGame.AppDataDirectory;
        await JsonListHelper.SerializeListToJsonAsync(games, _jsonPath, GamesSerializerContext.Default.ListGame);
    }
}