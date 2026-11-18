using SpeckitDemo.Data.Members;
using Microsoft.EntityFrameworkCore;

namespace SpeckitDemo.Data;

public class SpeckitDemoContext : DbContext
{
    public SpeckitDemoContext(DbContextOptions<SpeckitDemoContext> options) : base(options)
    {
    }

    public DbSet<Member> Users => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SpeckitDemoContext).Assembly);
    }
}
