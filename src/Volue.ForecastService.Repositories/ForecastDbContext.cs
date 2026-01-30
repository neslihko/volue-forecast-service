using Microsoft.EntityFrameworkCore;
using Volue.ForecastService.Repositories.Configurations;
using Volue.ForecastService.Repositories.Entities;

namespace Volue.ForecastService.Repositories;

/// <summary>
/// Database context for the Forecast Service.
/// </summary>
public class ForecastDbContext : DbContext
{
    public ForecastDbContext(DbContextOptions<ForecastDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<PowerPlant> PowerPlants => Set<PowerPlant>();
    public DbSet<Forecast> Forecasts => Set<Forecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new PowerPlantConfiguration());
        modelBuilder.ApplyConfiguration(new ForecastConfiguration());

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        // Seed Company
        var companyId = new Guid("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<Company>().HasData(new Company
        {
            Id = companyId,
            Name = "Energy Trading Corp",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Seed Power Plants
        var istanbulPlantId = new Guid("22222222-2222-2222-2222-222222222222");
        var sofiaPlantId = new Guid("33333333-3333-3333-3333-333333333333");
        var madridPlantId = new Guid("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<PowerPlant>().HasData(
            new PowerPlant
            {
                Id = istanbulPlantId,
                CompanyId = companyId,
                Name = "Istanbul Wind Farm",
                Country = "Turkey",
                CapacityMwh = 150.0000m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PowerPlant
            {
                Id = sofiaPlantId,
                CompanyId = companyId,
                Name = "Sofia Solar Park",
                Country = "Bulgaria",
                CapacityMwh = 200.0000m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PowerPlant
            {
                Id = madridPlantId,
                CompanyId = companyId,
                Name = "Madrid Hydro Station",
                Country = "Spain",
                CapacityMwh = 300.0000m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
