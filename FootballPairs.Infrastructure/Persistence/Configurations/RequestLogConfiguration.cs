using FootballPairs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballPairs.Infrastructure.Persistence.Configurations;

public sealed class RequestLogConfiguration : IEntityTypeConfiguration<RequestLog>
{
    public void Configure(EntityTypeBuilder<RequestLog> builder)
    {
        builder.ToTable("RequestLogs");
        builder.HasKey(requestLog => requestLog.Id);
        builder.Property(requestLog => requestLog.Id).ValueGeneratedOnAdd();
        builder.Property(requestLog => requestLog.Date).IsRequired();
        builder.Property(requestLog => requestLog.ErrorCode).IsRequired();
        builder.Property(requestLog => requestLog.Username).HasMaxLength(64);
        builder.Property(requestLog => requestLog.Data).IsRequired().HasColumnType("nvarchar(max)");
    }
}
