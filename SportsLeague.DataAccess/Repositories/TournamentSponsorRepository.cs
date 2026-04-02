using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces;

namespace SportsLeague.DataAccess.Repositories
{
    public class TournamentSponsorRepository
        : GenericRepository<TournamentSponsor>, ITournamentSponsorRepository
    {
        public TournamentSponsorRepository(LeagueDbContext context) : base(context)
        {
        }

        public async Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
        {
            return await _context.TournamentSponsors
                .Include(ts => ts.Tournament)
                .Include(ts => ts.Sponsor)
                .FirstOrDefaultAsync(ts =>
                    ts.TournamentId == tournamentId &&
                    ts.SponsorId == sponsorId);
        }

        public async Task<IEnumerable<TournamentSponsor>> GetBySponsorAsync(int sponsorId)
        {
            return await _context.TournamentSponsors
                .Include(ts => ts.Tournament)
                .Where(ts => ts.SponsorId == sponsorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TournamentSponsor>> GetByTournamentAsync(int tournamentId)
        {
            return await _context.TournamentSponsors
                .Include(ts => ts.Sponsor)
                .Where(ts => ts.TournamentId == tournamentId)
                .ToListAsync();
        }
    }
}