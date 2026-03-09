using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(match => match.Id);
        builder.Property(match => match.Id).ValueGeneratedOnAdd();
        builder.Property(match => match.MatchDate).IsRequired();
        builder.Property(match => match.HomeTeamId).IsRequired();
        builder.Property(match => match.AwayTeamId).IsRequired();
        builder.Property(match => match.Score).IsRequired().HasMaxLength(20);
        builder.Property(match => match.EndMinute).IsRequired().HasDefaultValue(90);
        builder.HasOne(match => match.HomeTeam)
            .WithMany()
            .HasForeignKey(match => match.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(match => match.AwayTeam)
            .WithMany()
            .HasForeignKey(match => match.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
