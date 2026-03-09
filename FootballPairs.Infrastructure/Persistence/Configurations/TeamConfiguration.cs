using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(team => team.Id);
        builder.Property(team => team.Id).ValueGeneratedOnAdd();
        builder.Property(team => team.Name).IsRequired().HasMaxLength(100);
        builder.Property(team => team.ManagerFullName).IsRequired().HasMaxLength(150);
        builder.Property(team => team.Group).IsRequired().HasMaxLength(20);
    }
}
