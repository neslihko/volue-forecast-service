using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volue.ForecastService.Repositories.Entities;

namespace Volue.ForecastService.Repositories.Configurations;

/// <summary>
/// Entity Framework configuration for the Forecast entity.
/// </summary>
public class ForecastConfiguration : IEntityTypeConfiguration<Forecast>
{
    public void Configure(EntityTypeBuilder<Forecast> builder)
    {
        builder.ToTable("forecasts");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(f => f.PlantId)
            .HasColumnName("plant_id")
            .IsRequired();

        builder.Property(f => f.HourUtc)
            .HasColumnName("hour_utc")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(f => f.Mwh)
            .HasColumnName("mwh")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // UPSERT constraint: Unique index on (plant_id, hour_utc)
        builder.HasIndex(f => new { f.PlantId, f.HourUtc })
            .IsUnique()
            .HasDatabaseName("uq_forecast_plant_hour");

        // Performance indexes
        builder.HasIndex(f => new { f.PlantId, f.HourUtc })
            .HasDatabaseName("idx_forecasts_plant_hour");

        builder.HasIndex(f => f.HourUtc)
            .HasDatabaseName("idx_forecasts_hour");
    }
}
