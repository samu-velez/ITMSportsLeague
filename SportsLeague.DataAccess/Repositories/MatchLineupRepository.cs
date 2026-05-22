using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class MatchLineupRepository : GenericRepository<MatchLineup>, IMatchLineupRepository
{
    public MatchLineupRepository(LeagueDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchIdAsync(int matchId)
    {
        return await _dbSet
            .Where(ml => ml.MatchId == matchId)
            .Include(ml => ml.Match)
            .Include(ml => ml.Player)
                .ThenInclude(p => p.Team)
            .OrderByDescending(ml => ml.IsStarter)
            .ThenBy(ml => ml.Player.Team.Name)
            .ThenBy(ml => ml.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<MatchLineup>> GetByTeamIdAsync(int matchId, int teamId)
    {
        return await _dbSet
            .Where(ml =>
                ml.MatchId == matchId &&
                ml.Player.TeamId == teamId &&
                (
                    ml.Match.HomeTeamId == teamId ||
                    ml.Match.AwayTeamId == teamId
                ))
            .Include(ml => ml.Match)
            .Include(ml => ml.Player)
                .ThenInclude(p => p.Team)
            .OrderByDescending(ml => ml.IsStarter)
            .ThenBy(ml => ml.Position)
            .ToListAsync();
    }
}