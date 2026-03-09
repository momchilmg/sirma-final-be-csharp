using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.Players.Models;
using FootballPairs.Application.Teams;
using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Players;

public sealed class PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository) : IPlayerService
{
    public async Task<PlayerDto> CreateAsync(CreatePlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await BuildPlayerAsync(command, cancellationToken);
        await playerRepository.AddAsync(player, cancellationToken);
        try
        {
            await playerRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ValidationException("Referenced team does not exist.");
        }

        return Map(player);
    }

    public async Task QueueCreateForImportAsync(CreatePlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await BuildPlayerAsync(command, cancellationToken);
        await playerRepository.AddAsync(player, cancellationToken);
    }

    public async Task FlushImportAsync(CancellationToken cancellationToken)
    {
        try
        {
            await playerRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ValidationException("Referenced team does not exist.");
        }
    }

    public async Task<IReadOnlyList<PlayerDto>> ListAsync(CancellationToken cancellationToken)
    {
        var players = await playerRepository.ListAsync(cancellationToken);
        return players.Select(Map).ToList();
    }

    public async Task<PlayerDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (player is null)
        {
            throw new NotFoundException("Player was not found.");
        }

        return Map(player);
    }

    public async Task<PlayerDto> UpdateAsync(int id, UpdatePlayerCommand command, CancellationToken cancellationToken)
    {
        Validate(command.TeamNumber, command.Position, command.FullName, command.TeamId);
        if (!await teamRepository.TeamExistsAsync(command.TeamId, cancellationToken))
        {
            throw new ValidationException("Referenced team does not exist.");
        }

        var player = await playerRepository.GetByIdAsync(id, cancellationToken);
        if (player is null)
        {
            throw new NotFoundException("Player was not found.");
        }

        player.TeamNumber = command.TeamNumber;
        player.Position = command.Position.Trim();
        player.FullName = command.FullName.Trim();
        player.TeamId = command.TeamId;
        try
        {
            await playerRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ValidationException("Referenced team does not exist.");
        }

        return Map(player);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdAsync(id, cancellationToken);
        if (player is null)
        {
            throw new NotFoundException("Player was not found.");
        }

        await playerRepository.DeleteAsync(player, cancellationToken);
        try
        {
            await playerRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ConflictException("Player cannot be deleted while match records are assigned.");
        }
    }

    private static void Validate(int teamNumber, string position, string fullName, int teamId)
    {
        if (teamNumber <= 0)
        {
            throw new ValidationException("Team number must be positive.");
        }

        if (teamId <= 0)
        {
            throw new ValidationException("Team id must be positive.");
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            throw new ValidationException("Position is required.");
        }

        var normalizedPosition = position.Trim();
        if (normalizedPosition.Length > 10)
        {
            throw new ValidationException("Position must be at most 10 characters.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ValidationException("Full name is required.");
        }

        var normalizedFullName = fullName.Trim();
        if (normalizedFullName.Length > 150)
        {
            throw new ValidationException("Full name must be at most 150 characters.");
        }
    }

    private async Task<Player> BuildPlayerAsync(CreatePlayerCommand command, CancellationToken cancellationToken)
    {
        Validate(command.TeamNumber, command.Position, command.FullName, command.TeamId);
        if (!await teamRepository.TeamExistsAsync(command.TeamId, cancellationToken))
        {
            throw new ValidationException("Referenced team does not exist.");
        }

        return new Player
        {
            TeamNumber = command.TeamNumber,
            Position = command.Position.Trim(),
            FullName = command.FullName.Trim(),
            TeamId = command.TeamId
        };
    }

    private static PlayerDto Map(Player player)
    {
        return new PlayerDto(player.Id, player.TeamNumber, player.Position, player.FullName, player.TeamId);
    }
}
