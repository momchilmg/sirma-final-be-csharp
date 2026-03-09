using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Username).IsRequired().HasMaxLength(64);
        builder.HasIndex(user => user.Username).IsUnique();
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.Property(user => user.PasswordSalt).IsRequired();
        builder.Property(user => user.Iterations).IsRequired();
        builder.Property(user => user.Role).IsRequired().HasMaxLength(16);
        builder.Property(user => user.CreatedAt).IsRequired();
    }
}
