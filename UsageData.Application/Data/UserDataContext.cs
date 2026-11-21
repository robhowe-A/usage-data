using Microsoft.EntityFrameworkCore;
using UsageData.Domain.Entities;

namespace UsageData.Data;

public class UserDataContext : DbContext
{
    public UserDataContext(DbContextOptions<UserDataContext> options)
        : base(options)
    {
    }

    public DbSet<UserAgent> UserAgents { get; set; }
    public DbSet<AnonymousUser> AnonymousUsers { get; set; }
    public DbSet<AccessRecord> AccessRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDataContext).Assembly);
    }
}
