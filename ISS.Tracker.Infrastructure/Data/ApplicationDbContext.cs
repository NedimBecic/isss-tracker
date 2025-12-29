using ISS.Tracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ISS.Tracker.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Satellite> Satellites => Set<Satellite>();
    public DbSet<Launch> Launches => Set<Launch>();
    public DbSet<LaunchStatistics> LaunchStatistics => Set<LaunchStatistics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Satellite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NoradId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<Launch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LaunchLibraryId).IsUnique();
            entity.Property(e => e.LaunchLibraryId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RocketName).HasMaxLength(200);
            entity.Property(e => e.LaunchProvider).HasMaxLength(200);
            entity.Property(e => e.MissionDescription).HasMaxLength(5000);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.VideoUrl).HasMaxLength(1000);
            entity.Property(e => e.LaunchSite).HasMaxLength(500);
        });

        modelBuilder.Entity<LaunchStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.LaunchesByProviderJson).HasMaxLength(2000);
            entity.Property(e => e.LaunchesByMonthJson).HasMaxLength(2000);
        });

        modelBuilder.Entity<Satellite>().HasData(new Satellite
        {
            Id = 1,
            NoradId = 25544,
            Name = "International Space Station (ISS)",
            Type = SatelliteType.SpaceStation,
            LaunchDate = new DateTime(1998, 11, 20, 0, 0, 0, DateTimeKind.Utc),
            Description = "The International Space Station is a modular space station in low Earth orbit. It is a multinational collaborative project involving five participating space agencies: NASA (United States), Roscosmos (Russia), JAXA (Japan), ESA (Europe), and CSA (Canada).",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
