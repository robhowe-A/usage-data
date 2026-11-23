namespace UsageData.Application.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsageData.Domain.Entities;

public sealed class AccessRecordConfiguration : IEntityTypeConfiguration<AccessRecord>
{
    public void Configure(EntityTypeBuilder<AccessRecord> builder)
    {
        builder.ToTable("Accessed_By");
        builder.HasKey(x => new { x.AnonymousUserId, x.UserAgentId });
    }
}