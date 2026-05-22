using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services
{
    public interface IMatchLineupService
    {
        Task<MatchLineup?> GetByIdAsync(int Id);
        Task<MatchLineup> CreateAsync(MatchLineup matchLineup);
        Task UpdateAsync(int id, MatchLineup matchLineup);
        Task DeleteAsync(int Id);

        Task<IEnumerable<MatchLineup>> GetByMatchIdAsync(int matchId);

        Task<IEnumerable<MatchLineup>> GetByTeamIdAsync(int matchId, int teamId);
    }
}