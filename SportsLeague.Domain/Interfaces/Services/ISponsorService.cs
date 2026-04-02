using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces
{
    public interface ISponsorService
    {
        Task<IEnumerable<Sponsor>> GetAllAsync();

        Task<Sponsor?> GetByIdAsync(int id);

        Task<Sponsor> CreateAsync(Sponsor sponsor);

        Task<Sponsor> UpdateAsync(int id, Sponsor sponsor);

        Task<bool> DeleteAsync(int id);

        Task LinkSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount);

        Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId);

        Task<bool> UnlinkSponsorFromTournamentAsync(int sponsorId, int tournamentId);
    }
}
