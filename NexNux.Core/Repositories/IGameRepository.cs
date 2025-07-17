using NexNux.Core.Models;

namespace NexNux.Core.Repositories;

public interface IGameRepository
{
    public Task<List<Game>> GetGames();
    public Task<Game?> GetGameById(Guid gameId);
    public Task AddGame(Game game);
    public Task RemoveGameById(Guid gameId);
    public Task ModifyGame(Game game);
}