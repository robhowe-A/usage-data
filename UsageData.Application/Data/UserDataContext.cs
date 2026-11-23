using Microsoft.EntityFrameworkCore;
using UsageData.Domain.Entities;

namespace UsageData.Application.Data;

public class UserDataContext(DbContextOptions<UserDataContext> options) : DbContext(options)
{
    public DbSet<UserAgent> UserAgents { get; set; }
    public DbSet<AnonymousUser> AnonymousUsers { get; set; }
    public DbSet<AccessRecord> AccessRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDataContext).Assembly);
    }
}
