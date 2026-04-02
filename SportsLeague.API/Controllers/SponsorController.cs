using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController : ControllerBase
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly IMapper _mapper;

        public SponsorController(
            ISponsorRepository sponsorRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ITournamentRepository tournamentRepository,
            IMapper mapper)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _tournamentRepository = tournamentRepository;
            _mapper = mapper;
        }

        // GET: api/sponsor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
        {
            var sponsors = await _sponsorRepository.GetAllAsync();
            var response = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);

            return Ok(response);
        }

        // GET: api/sponsor/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)
                return NotFound("Sponsor no encontrado");

            var response = _mapper.Map<SponsorResponseDTO>(sponsor);

            return Ok(response);
        }

        // POST: api/sponsor
        [HttpPost]
        public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest("El nombre es obligatorio");

                if (string.IsNullOrWhiteSpace(request.ContactEmail))
                    return BadRequest("El email es obligatorio");

                if (!request.ContactEmail.Contains("@"))
                    return BadRequest("Email inválido");

                if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Length < 7)
                    return BadRequest("Teléfono inválido");

                if (string.IsNullOrWhiteSpace(request.WebsiteUrl) ||
                    !request.WebsiteUrl.StartsWith("http"))
                    return BadRequest("URL inválida");

                var sponsor = _mapper.Map<Sponsor>(request);

                var createdSponsor = await _sponsorRepository.CreateAsync(sponsor);

                var response = _mapper.Map<SponsorResponseDTO>(createdSponsor);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = response.Id },
                    response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al crear sponsor: {ex.Message}");
            }
        }

        // PUT: api/sponsor/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SponsorRequestDTO request)
        {
            try
            {
                var exists = await _sponsorRepository.ExistsAsync(id);

                if (!exists)
                    return NotFound("Sponsor no encontrado");

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest("El nombre es obligatorio");

                if (string.IsNullOrWhiteSpace(request.ContactEmail))
                    return BadRequest("El email es obligatorio");

                if (!request.ContactEmail.Contains("@"))
                    return BadRequest("Email inválido");

                if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Length < 7)
                    return BadRequest("Teléfono inválido");

                if (string.IsNullOrWhiteSpace(request.WebsiteUrl) ||
                    !request.WebsiteUrl.StartsWith("http"))
                    return BadRequest("URL inválida");

                var sponsor = _mapper.Map<Sponsor>(request);
                sponsor.Id = id;

                await _sponsorRepository.UpdateAsync(sponsor);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al actualizar: {ex.Message}");
            }
        }

        // DELETE: api/sponsor/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exists = await _sponsorRepository.ExistsAsync(id);

                if (!exists)
                    return NotFound("Sponsor no encontrado");

                await _sponsorRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al eliminar: {ex.Message}");
            }
        }

        // POST: api/sponsor/{id}/tournaments
        [HttpPost("{id}/tournaments")]
        public async Task<IActionResult> AddTournament(int id, TournamentSponsorRequestDTO request)
        {
            try
            {
                var sponsorExists = await _sponsorRepository.ExistsAsync(id);

                if (!sponsorExists)
                    return NotFound("Sponsor no existe");

                var tournamentExists = await _tournamentRepository.ExistsAsync(request.TournamentId);

                if (!tournamentExists)
                    return NotFound("Tournament no existe");

                if (request.ContractAmount <= 0)
                    return BadRequest("El contrato debe ser mayor a 0");

                var relationExists = await _tournamentSponsorRepository
                    .GetByTournamentAndSponsorAsync(request.TournamentId, id);

                if (relationExists != null)
                    return BadRequest("El sponsor ya está registrado en este torneo");

                var relation = new TournamentSponsor
                {
                    SponsorId = id,
                    TournamentId = request.TournamentId,
                    ContractAmount = request.ContractAmount,
                    JoinedAt = DateTime.UtcNow
                };

                await _tournamentSponsorRepository.CreateAsync(relation);

                return Ok("Sponsor agregado al torneo correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // GET: api/sponsor/{id}/tournaments
        [HttpGet("{id}/tournaments")]
        public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetTournaments(int id)
        {
            var sponsorExists = await _sponsorRepository.ExistsAsync(id);

            if (!sponsorExists)
                return NotFound("Sponsor no encontrado");

            var tournaments = await _tournamentSponsorRepository.GetBySponsorAsync(id);

            var response = _mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(tournaments);

            return Ok(response);
        }

        // DELETE: api/sponsor/{id}/tournaments/{tournamentId}
        [HttpDelete("{id}/tournaments/{tournamentId}")]
        public async Task<IActionResult> RemoveTournament(int id, int tournamentId)
        {
            try
            {
                var relation = await _tournamentSponsorRepository
                    .GetByTournamentAndSponsorAsync(tournamentId, id);

                if (relation == null)
                    return NotFound("Relación no encontrada");

                await _tournamentSponsorRepository.DeleteAsync(relation.Id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}