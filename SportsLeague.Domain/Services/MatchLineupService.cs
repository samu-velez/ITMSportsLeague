using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository matchLineupRepository,
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ILogger<MatchLineupService> logger)
    {
        _matchLineupRepository = matchLineupRepository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<MatchLineup?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving match lineup with ID: {MatchLineupId}", id);
        return await _matchLineupRepository.GetByIdAsync(id);
    }

    public async Task<MatchLineup> CreateAsync(MatchLineup matchLineup)
    {
        // 1. Validar que el partido existe
        var match = await _matchRepository.GetByIdAsync(matchLineup.MatchId);
        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchLineup.MatchId}");

        // 2. Validar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(matchLineup.PlayerId);
        if (player == null)
            throw new KeyNotFoundException(
                $"No se encontró el jugador con ID {matchLineup.PlayerId}");

        // 3. Validar que el partido esté en estado Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo se pueden registrar alineaciones en partidos Scheduled");

        // 4. Validar que el jugador pertenece al HomeTeam o AwayTeam del partido
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            throw new InvalidOperationException(
                "El jugador no pertenece a ninguno de los equipos del partido");

        // 5. Validar que el jugador no esté registrado dos veces
        var lineup = await _matchLineupRepository.GetByMatchIdAsync(matchLineup.MatchId);

        var playerAlreadyRegistered = lineup.Any(x => x.PlayerId == matchLineup.PlayerId);
        if (playerAlreadyRegistered)
            throw new InvalidOperationException(
                "El jugador ya está registrado en la alineación de este partido");

        // 6. Validar máximo 11 titulares por equipo por partido
        if (matchLineup.IsStarter)
        {
            var startersCount = lineup.Count(x =>
                x.IsStarter &&
                x.Player.TeamId == player.TeamId);

            if (startersCount >= 11)
                throw new InvalidOperationException(
                    "El equipo ya tiene 11 titulares registrados en este partido");
        }

        _logger.LogInformation(
            "Creating match lineup: Match {MatchId}, Player {PlayerId}",
            matchLineup.MatchId, matchLineup.PlayerId);

        return await _matchLineupRepository.CreateAsync(matchLineup);
    }

    public async Task UpdateAsync(int id, MatchLineup matchLineup)
    {
        var existing = await _matchLineupRepository.GetByIdAsync(id);

        if (existing == null)
            throw new KeyNotFoundException(
                $"No se encontró la alineación con ID {id}");

        existing.PlayerId = matchLineup.PlayerId;
        existing.MatchId = matchLineup.MatchId;
        existing.IsStarter = matchLineup.IsStarter;
        existing.Position = matchLineup.Position;

        _logger.LogInformation(
            "Updating match lineup with ID: {MatchLineupId}", id);

        await _matchLineupRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _matchLineupRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException(
                $"No se encontró la alineación con ID {id}");

        _logger.LogInformation(
            "Deleting match lineup with ID: {MatchLineupId}", id);

        await _matchLineupRepository.DeleteAsync(id);
    }
    public async Task<IEnumerable<MatchLineup>> GetByMatchIdAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchId}");

        _logger.LogInformation(
            "Retrieving lineup for match ID: {MatchId}", matchId);

        return await _matchLineupRepository.GetByMatchIdAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetByTeamIdAsync(int matchId, int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchId}");

        if (match.HomeTeamId != teamId && match.AwayTeamId != teamId)
            throw new InvalidOperationException(
                "El equipo no pertenece a este partido");

        _logger.LogInformation(
            "Retrieving lineup for match ID: {MatchId} and team ID: {TeamId}",
            matchId,
            teamId);

        return await _matchLineupRepository.GetByTeamIdAsync(matchId, teamId);
    }
}