using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class FootballPairsDbContext(DbContextOptions<FootballPairsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchRecord> MatchRecords => Set<MatchRecord>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FootballPairsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
