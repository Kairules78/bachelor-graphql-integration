using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CountryByName> CountryByNames => Set<CountryByName>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CountryByName>()
            .HasIndex(x => new { x.NameQuery, x.FetchedAtUtc });
    }
}

