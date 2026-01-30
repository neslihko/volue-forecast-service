namespace Volue.ForecastService.Contracts.Models;

/// <summary>
/// Represents the result of a bulk UPSERT operation.
/// </summary>
public record UpsertResult
{
    /// <summary>
    /// Number of records inserted (new forecasts).
    /// </summary>
    public int InsertedCount { get; init; }

    /// <summary>
    /// Number of records updated (existing forecasts with changed values).
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// Number of records that were unchanged (existing forecasts with same values).
    /// </summary>
    public int UnchangedCount { get; init; }

    /// <summary>
    /// Total number of records that were modified (inserted or updated).
    /// </summary>
    public int TotalChanged => InsertedCount + UpdatedCount;

    /// <summary>
    /// Indicates whether any changes were made to the database.
    /// </summary>
    public bool HasChanges => TotalChanged > 0;

    /// <summary>
    /// Total number of records processed.
    /// </summary>
    public int TotalProcessed => InsertedCount + UpdatedCount + UnchangedCount;
}
