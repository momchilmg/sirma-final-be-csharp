using System.ComponentModel.DataAnnotations;
using FootballPairs.Api.Contracts.Requests.Teams;
using FootballPairs.Api.Contracts.Responses.Teams;
using FootballPairs.Application.Teams;
using FootballPairs.Application.Teams.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public sealed class TeamsController(ITeamService teamService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamResponse>> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTeamCommand(request.Name, request.ManagerFullName, request.Group);
        var team = await teamService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, Map(team));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TeamResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamResponse>>> List(CancellationToken cancellationToken)
    {
        var teams = await teamService.ListAsync(cancellationToken);
        return Ok(teams.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamResponse>> GetById([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var team = await teamService.GetByIdAsync(id, cancellationToken);
        return Ok(Map(team));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamResponse>> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateTeamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTeamCommand(request.Name, request.ManagerFullName, request.Group);
        var team = await teamService.UpdateAsync(id, command, cancellationToken);
        return Ok(Map(team));
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
        await teamService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static TeamResponse Map(TeamDto team)
    {
        return new TeamResponse(team.Id, team.Name, team.ManagerFullName, team.Group);
    }
}
