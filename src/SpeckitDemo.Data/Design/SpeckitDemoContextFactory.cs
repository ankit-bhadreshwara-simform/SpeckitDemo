using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpeckitDemo.Data.Design;

public sealed class SpeckitDemoContextFactory : IDesignTimeDbContextFactory<SpeckitDemoContext>
{
    public SpeckitDemoContext CreateDbContext(string[] args)
    {
        var root = LocateRepositoryRoot();
        var databasePath = Path.Combine(root, "src", "SpeckitDemo.Api", "speckitdemo.db");

        var options = new DbContextOptionsBuilder<SpeckitDemoContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new SpeckitDemoContext(options);
    }

    private static string LocateRepositoryRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "SpeckitDemo.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException("SpeckitDemo.sln was not found from the design-time working directory.");
    }
}
