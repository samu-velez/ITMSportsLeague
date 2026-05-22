using SportsLeague.Domain.Entities;
namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface IMatchLineupRepository : IGenericRepository<MatchLineup>
    {
        Task<IEnumerable<MatchLineup>> GetByMatchIdAsync(int matchId);
        Task<IEnumerable<MatchLineup>> GetByTeamIdAsync(int matchId, int teamId);

    }
}