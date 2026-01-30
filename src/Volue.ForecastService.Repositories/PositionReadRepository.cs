using Microsoft.EntityFrameworkCore;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;

namespace Volue.ForecastService.Repositories;

/// <summary>
/// Repository implementation for reading company position data with aggregation.
/// </summary>
public class PositionReadRepository : IPositionReadRepository
{
    private readonly ForecastDbContext _context;

    public PositionReadRepository(ForecastDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CompanyPosition?> GetCompanyPositionAsync(
        Guid companyId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        // First, check if the company exists
        var company = await _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId && c.IsActive)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (company == null)
        {
            return null;
        }

        // Aggregate forecasts across all power plants for this company
        var hourlyPositions = await _context.Forecasts
            .AsNoTracking()
            .Where(f => f.Plant.CompanyId == companyId
                && f.Plant.IsActive
                && f.HourUtc >= fromUtc
                && f.HourUtc < toUtc)
            .GroupBy(f => f.HourUtc)
            .Select(g => new HourlyPosition
            {
                HourUtc = g.Key,
                TotalMwh = g.Sum(f => f.Mwh),
                PlantCount = g.Select(f => f.PlantId).Distinct().Count()
            })
            .OrderBy(h => h.HourUtc)
            .ToListAsync(cancellationToken);

        return new CompanyPosition
        {
            CompanyId = company.Id,
            CompanyName = company.Name,
            Positions = hourlyPositions
        };
    }

    public async Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .AsNoTracking()
            .AnyAsync(c => c.Id == companyId && c.IsActive, cancellationToken);
    }
}
