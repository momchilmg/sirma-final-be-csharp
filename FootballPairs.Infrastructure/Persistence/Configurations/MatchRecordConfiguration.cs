using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class MatchRecordConfiguration : IEntityTypeConfiguration<MatchRecord>
{
    public void Configure(EntityTypeBuilder<MatchRecord> builder)
    {
        builder.ToTable("MatchRecords", tableBuilder => tableBuilder.HasTrigger("TR_MatchRecords_NoOverlap"));
        builder.HasKey(matchRecord => matchRecord.Id);
        builder.Property(matchRecord => matchRecord.Id).ValueGeneratedOnAdd();
        builder.Property(matchRecord => matchRecord.MatchId).IsRequired();
        builder.Property(matchRecord => matchRecord.PlayerId).IsRequired();
        builder.Property(matchRecord => matchRecord.FromMinute).IsRequired();
        builder.Property(matchRecord => matchRecord.ToMinute);
        builder.HasIndex(matchRecord => new { matchRecord.MatchId, matchRecord.PlayerId }).IsUnique();
        builder.HasOne(matchRecord => matchRecord.Match)
            .WithMany()
            .HasForeignKey(matchRecord => matchRecord.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(matchRecord => matchRecord.Player)
            .WithMany()
            .HasForeignKey(matchRecord => matchRecord.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
