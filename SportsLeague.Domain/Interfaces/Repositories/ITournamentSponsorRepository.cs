using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces
{
    public interface ITournamentSponsorRepository : IGenericRepository<TournamentSponsor>
    {
        Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId);

        Task<IEnumerable<TournamentSponsor>> GetBySponsorAsync(int sponsorId);

        Task<IEnumerable<TournamentSponsor>> GetByTournamentAsync(int tournamentId);
    }
}