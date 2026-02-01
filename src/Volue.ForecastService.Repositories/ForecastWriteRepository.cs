using Microsoft.EntityFrameworkCore;
using Npgsql;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;
using Volue.ForecastService.Repositories.Entities;

namespace Volue.ForecastService.Repositories;

/// <summary>
/// Repository implementation for writing forecasts with PostgreSQL UPSERT.
/// </summary>
public class ForecastWriteRepository : IForecastWriteRepository
{
    private readonly ForecastDbContext _context;

    public ForecastWriteRepository(ForecastDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UpsertResult> UpsertForecastsAsync(
        Guid plantId,
        IEnumerable<ForecastPoint> points,
        CancellationToken cancellationToken = default)
    {
        var pointsList = points.ToList();
        if (pointsList.Count == 0)
        {
            return new UpsertResult
            {
                InsertedCount = 0,
                UpdatedCount = 0,
                UnchangedCount = 0
            };
        }

        var now = DateTime.UtcNow;

        // Use PostgreSQL-specific UPSERT with ON CONFLICT
        // This approach uses ExecuteSqlRaw with parameters to prevent SQL injection
        var sql = @"
            WITH input_data AS (
                SELECT 
                    @plantId::uuid as plant_id,
                    unnest(@hours::timestamptz[]) as hour_utc,
                    unnest(@mwhs::numeric[]) as mwh
            ),
            upsert AS (
                INSERT INTO forecasts (id, plant_id, hour_utc, mwh, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    plant_id,
                    hour_utc,
                    mwh,
                    @now::timestamptz,
                    @now::timestamptz
                FROM input_data
                ON CONFLICT (plant_id, hour_utc) 
                DO UPDATE SET 
                    mwh = EXCLUDED.mwh,
                    updated_at = @now::timestamptz
                WHERE forecasts.mwh IS DISTINCT FROM EXCLUDED.mwh
                RETURNING 
                    (xmax::text::int = 0) AS was_inserted,
                    (xmax::text::int > 0) AS was_updated
            )
            SELECT 
                COUNT(*) FILTER (WHERE was_inserted = true) AS inserted_count,
                COUNT(*) FILTER (WHERE was_updated = true) AS updated_count,
                @totalCount - COUNT(*) AS unchanged_count
            FROM upsert;
        ";

        var hours = pointsList.Select(p => p.HourUtc).ToArray();
        var mwhs = pointsList.Select(p => p.Mwh).ToArray();
        var totalCount = pointsList.Count;

        var parameters = new[]
        {
            new NpgsqlParameter("@plantId", plantId),
            new NpgsqlParameter("@hours", hours),
            new NpgsqlParameter("@mwhs", mwhs),
            new NpgsqlParameter("@now", now),
            new NpgsqlParameter("@totalCount", totalCount)
        };

        // Execute the query and read the result
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);

        await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var insertedCount = reader.GetInt32(0);
                var updatedCount = reader.GetInt32(1);
                var unchangedCount = reader.GetInt32(2);

                return new UpsertResult
                {
                    InsertedCount = insertedCount,
                    UpdatedCount = updatedCount,
                    UnchangedCount = unchangedCount
                };
            }

            return new UpsertResult
            {
                InsertedCount = 0,
                UpdatedCount = 0,
                UnchangedCount = totalCount
            };
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
}
