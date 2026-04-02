using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Services.Implementations
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentRepository tournamentRepository,
            ITournamentSponsorRepository tournamentSponsorRepository)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return await _sponsorRepository.GetByIdAsync(id);
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            if (string.IsNullOrWhiteSpace(sponsor.Name))
                throw new ArgumentException("El nombre del sponsor es obligatorio");

            if (!sponsor.ContactEmail.Contains("@"))
                throw new ArgumentException("Email inválido");

            var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);

            if (exists)
                throw new InvalidOperationException("Ya existe un sponsor con ese nombre");

            sponsor.CreatedAt = DateTime.UtcNow;
            sponsor.UpdatedAt = DateTime.UtcNow;

            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task<Sponsor> UpdateAsync(int id, Sponsor sponsor)
        {
            var existingSponsor = await _sponsorRepository.GetByIdAsync(id);

            if (existingSponsor == null)
                throw new KeyNotFoundException("Sponsor no encontrado");

            if (string.IsNullOrWhiteSpace(sponsor.Name))
                throw new ArgumentException("El nombre es obligatorio");

            if (!sponsor.ContactEmail.Contains("@"))
                throw new ArgumentException("Email inválido");

            existingSponsor.Name = sponsor.Name;
            existingSponsor.ContactEmail = sponsor.ContactEmail;
            existingSponsor.Phone = sponsor.Phone;
            existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
            existingSponsor.Category = sponsor.Category;
            existingSponsor.UpdatedAt = DateTime.UtcNow;

            await _sponsorRepository.UpdateAsync(existingSponsor);

            return existingSponsor;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)
                throw new KeyNotFoundException("Sponsor no encontrado");

            await _sponsorRepository.DeleteAsync(id);

            return true;
        }

        public async Task LinkSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            if (contractAmount <= 0)
                throw new ArgumentException("El monto del contrato debe ser mayor a 0");

            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);

            if (sponsor == null)
                throw new KeyNotFoundException("Sponsor no encontrado");

            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);

            if (tournament == null)
                throw new KeyNotFoundException("Tournament no encontrado");

            var existingRelation = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

            if (existingRelation != null)
                throw new InvalidOperationException("El sponsor ya está vinculado a este torneo");

            var relation = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tournamentSponsorRepository.CreateAsync(relation);
        }

        public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);

            if (sponsor == null)
                throw new KeyNotFoundException("Sponsor no encontrado");

            return await _tournamentSponsorRepository
                .GetBySponsorAsync(sponsorId);
        }

        public async Task<bool> UnlinkSponsorFromTournamentAsync(int sponsorId, int tournamentId)
        {
            var relation = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

            if (relation == null)
                throw new KeyNotFoundException("Relación no encontrada");

            await _tournamentSponsorRepository.DeleteAsync(relation.Id);

            return true;
        }
    }
}