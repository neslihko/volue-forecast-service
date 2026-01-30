using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volue.ForecastService.Repositories.Entities;

namespace Volue.ForecastService.Repositories.Configurations;

/// <summary>
/// Entity Framework configuration for the PowerPlant entity.
/// </summary>
public class PowerPlantConfiguration : IEntityTypeConfiguration<PowerPlant>
{
    public void Configure(EntityTypeBuilder<PowerPlant> builder)
    {
        builder.ToTable("power_plants");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Country)
            .HasColumnName("country")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.CapacityMwh)
            .HasColumnName("capacity_mwh")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Relationships
        builder.HasMany(p => p.Forecasts)
            .WithOne(f => f.Plant)
            .HasForeignKey(f => f.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
