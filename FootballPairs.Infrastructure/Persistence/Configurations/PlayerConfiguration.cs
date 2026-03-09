using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");
        builder.HasKey(player => player.Id);
        builder.Property(player => player.Id).ValueGeneratedOnAdd();
        builder.Property(player => player.TeamNumber).IsRequired();
        builder.Property(player => player.Position).IsRequired().HasMaxLength(10);
        builder.Property(player => player.FullName).IsRequired().HasMaxLength(150);
        builder.Property(player => player.TeamId).IsRequired();
        builder.HasOne(player => player.Team)
            .WithMany(team => team.Players)
            .HasForeignKey(player => player.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
