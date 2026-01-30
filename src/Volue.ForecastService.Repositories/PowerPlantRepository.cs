using Microsoft.EntityFrameworkCore;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;

namespace Volue.ForecastService.Repositories;

/// <summary>
/// Repository implementation for reading power plant data.
/// </summary>
public class PowerPlantRepository : IPowerPlantRepository
{
    private readonly ForecastDbContext _context;

    public PowerPlantRepository(ForecastDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<PowerPlantInfo>> GetAllActivePlantsAsync(
        CancellationToken cancellationToken = default)
    {
        var plants = await _context.PowerPlants
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new PowerPlantInfo
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                Name = p.Name,
                Country = p.Country,
                CapacityMwh = p.CapacityMwh,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return plants;
    }

    public async Task<PowerPlantInfo?> GetPlantByIdAsync(
        Guid plantId,
        CancellationToken cancellationToken = default)
    {
        var plant = await _context.PowerPlants
            .AsNoTracking()
            .Where(p => p.Id == plantId)
            .Select(p => new PowerPlantInfo
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                Name = p.Name,
                Country = p.Country,
                CapacityMwh = p.CapacityMwh,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return plant;
    }
}
