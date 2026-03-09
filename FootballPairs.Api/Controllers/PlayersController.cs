using System.ComponentModel.DataAnnotations;
using FootballPairs.Api.Contracts.Requests.Players;
using FootballPairs.Api.Contracts.Responses.Players;
using FootballPairs.Application.Players;
using FootballPairs.Application.Players.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/players")]
[Authorize]
public sealed class PlayersController(IPlayerService playerService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlayerResponse>> Create([FromBody] CreatePlayerRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePlayerCommand(request.TeamNumber, request.Position, request.FullName, request.TeamId);
        var player = await playerService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = player.Id }, Map(player));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PlayerResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlayerResponse>>> List(CancellationToken cancellationToken)
    {
        var players = await playerService.ListAsync(cancellationToken);
        return Ok(players.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> GetById([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var player = await playerService.GetByIdAsync(id, cancellationToken);
        return Ok(Map(player));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdatePlayerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePlayerCommand(request.TeamNumber, request.Position, request.FullName, request.TeamId);
        var player = await playerService.UpdateAsync(id, command, cancellationToken);
        return Ok(Map(player));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        await playerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static PlayerResponse Map(PlayerDto player)
    {
        return new PlayerResponse(player.Id, player.TeamNumber, player.Position, player.FullName, player.TeamId);
    }
}
