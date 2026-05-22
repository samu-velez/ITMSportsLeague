using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using SportsLeague.API.DTOs.Request;

using SportsLeague.API.DTOs.Response;

using SportsLeague.Domain.Entities;

using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;

    private readonly IMapper _mapper;

    public MatchLineupController(
        IMatchLineupService matchLineupService,
        IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetByMatchId(
        int matchId)
    {
        try
        {
            var lineup = await _matchLineupService.GetByMatchIdAsync(matchId);

            return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetByTeamId(
        int matchId,
        int teamId)
    {
        try
        {
            var lineup = await _matchLineupService
                .GetByTeamIdAsync(matchId, teamId);

            return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchLineupResponseDTO>> GetById(
        int matchId,
        int id)
    {
        var lineup = await _matchLineupService.GetByIdAsync(id);

        if (lineup == null)
            return NotFound(new
            {
                message = $"Alineación con ID {id} no encontrada"
            });

        if (lineup.MatchId != matchId)
            return BadRequest(new
            {
                message = "La alineación no pertenece a este partido"
            });

        return Ok(_mapper.Map<MatchLineupResponseDTO>(lineup));
    }

    [HttpPost]
    public async Task<ActionResult<MatchLineupResponseDTO>> Create(
        int matchId,
        MatchLineupRequestDTO dto)
    {
        try
        {
            var matchLineup = _mapper.Map<MatchLineup>(dto);

            matchLineup.MatchId = matchId;

            var created = await _matchLineupService.CreateAsync(matchLineup);

            var createdWithDetails =
                await _matchLineupService.GetByIdAsync(created.Id);

            var responseDto =
                _mapper.Map<MatchLineupResponseDTO>(createdWithDetails);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    matchId = responseDto.MatchId,
                    id = responseDto.Id
                },
                responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        int matchId,
        int id,
        MatchLineupRequestDTO dto)
    {
        try
        {
            var matchLineup = _mapper.Map<MatchLineup>(dto);

            matchLineup.MatchId = matchId;

            await _matchLineupService.UpdateAsync(id, matchLineup);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(
        int matchId,
        int id)
    {
        try
        {
            var lineup = await _matchLineupService.GetByIdAsync(id);

            if (lineup == null)
                return NotFound(new
                {
                    message = $"Alineación con ID {id} no encontrada"
                });

            if (lineup.MatchId != matchId)
                return BadRequest(new
                {
                    message = "La alineación no pertenece a este partido"
                });

            await _matchLineupService.DeleteAsync(id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}