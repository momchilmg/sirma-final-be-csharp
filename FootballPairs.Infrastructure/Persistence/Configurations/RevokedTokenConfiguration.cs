using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("RevokedTokens", tableBuilder => tableBuilder.HasTrigger("TR_RevokedTokens_DeleteExpiredAfter24Hours"));
        builder.HasKey(revokedToken => revokedToken.Id);
        builder.Property(revokedToken => revokedToken.Id).ValueGeneratedOnAdd();
        builder.Property(revokedToken => revokedToken.Jti).IsRequired().HasMaxLength(64);
        builder.HasIndex(revokedToken => revokedToken.Jti).IsUnique();
        builder.Property(revokedToken => revokedToken.ExpiresAtUtc).IsRequired();
        builder.Property(revokedToken => revokedToken.RevokedAtUtc).IsRequired();
    }
}
