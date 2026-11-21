namespace UsageData.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsageData.Domain.Entities;

public sealed class UserAgentConfiguration : IEntityTypeConfiguration<UserAgent>
{
    public void Configure(EntityTypeBuilder<UserAgent> builder)
    {
        builder.ToTable("User_Agent");
        builder.HasKey(x => x.AgentId); // not found by convention, so it must be explicit, RH 11/21/26
    }
}