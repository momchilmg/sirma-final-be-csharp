using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.Teams.Models;
using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Teams;

public sealed class TeamService(ITeamRepository teamRepository) : ITeamService
{
    public async Task<TeamDto> CreateAsync(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var team = BuildTeam(command);
        await teamRepository.AddAsync(team, cancellationToken);
        await teamRepository.SaveChangesAsync(cancellationToken);
        return Map(team);
    }

    public async Task QueueCreateForImportAsync(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var team = BuildTeam(command);
        await teamRepository.AddAsync(team, cancellationToken);
    }

    public Task FlushImportAsync(CancellationToken cancellationToken)
    {
        return teamRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeamDto>> ListAsync(CancellationToken cancellationToken)
    {
        var teams = await teamRepository.ListAsync(cancellationToken);
        return teams.Select(Map).ToList();
    }

    public async Task<TeamDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (team is null)
        {
            throw new NotFoundException("Team was not found.");
        }

        return Map(team);
    }

    public async Task<TeamDto> UpdateAsync(int id, UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        Validate(command.Name, command.ManagerFullName, command.Group);
        var team = await teamRepository.GetByIdAsync(id, cancellationToken);
        if (team is null)
        {
            throw new NotFoundException("Team was not found.");
        }

        team.Name = command.Name.Trim();
        team.ManagerFullName = command.ManagerFullName.Trim();
        team.Group = command.Group.Trim();
        await teamRepository.SaveChangesAsync(cancellationToken);
        return Map(team);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdAsync(id, cancellationToken);
        if (team is null)
        {
            throw new NotFoundException("Team was not found.");
        }

        if (await teamRepository.HasPlayersAsync(id, cancellationToken))
        {
            throw new ConflictException("Team cannot be deleted while players are assigned.");
        }

        if (await teamRepository.HasMatchesAsync(id, cancellationToken))
        {
            throw new ConflictException("Team cannot be deleted while matches are assigned.");
        }

        await teamRepository.DeleteAsync(team, cancellationToken);
        try
        {
            await teamRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ConflictException("Team cannot be deleted while players or matches are assigned.");
        }
    }

    private static void Validate(string name, string managerFullName, string group)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Team name is required.");
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length > 100)
        {
            throw new ValidationException("Team name must be at most 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(managerFullName))
        {
            throw new ValidationException("Manager full name is required.");
        }

        var normalizedManagerName = managerFullName.Trim();
        if (normalizedManagerName.Length > 150)
        {
            throw new ValidationException("Manager full name must be at most 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(group))
        {
            throw new ValidationException("Group is required.");
        }

        var normalizedGroup = group.Trim();
        if (normalizedGroup.Length > 20)
        {
            throw new ValidationException("Group must be at most 20 characters.");
        }
    }

    private static Team BuildTeam(CreateTeamCommand command)
    {
        Validate(command.Name, command.ManagerFullName, command.Group);
        return new Team
        {
            Name = command.Name.Trim(),
            ManagerFullName = command.ManagerFullName.Trim(),
            Group = command.Group.Trim()
        };
    }

    private static TeamDto Map(Team team)
    {
        return new TeamDto(team.Id, team.Name, team.ManagerFullName, team.Group);
    }
}
