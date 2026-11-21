namespace UsageData.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsageData.Domain.Entities;

public sealed class AnonymousUserConfiguration : IEntityTypeConfiguration<AnonymousUser>
{
    public void Configure(EntityTypeBuilder<AnonymousUser> builder)
    {
        builder.ToTable("Anonymous_User");
        builder.HasKey(x => x.AnonymousUserId);
        // No HasOne/foreign key: the relationship stays a plain ID (§4), RH 11/21/26
    }
}