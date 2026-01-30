namespace Volue.ForecastService.Contracts.Common;

/// <summary>
/// Centralized domain errors for the forecast service.
/// </summary>
public static class DomainErrors
{
    public static class Forecast
    {
        public static Error PlantNotFound(Guid plantId) =>
            Error.Create("Forecast.PlantNotFound", $"Power plant with ID '{plantId}' was not found or is inactive.");

        public static Error InvalidTimeRange =>
            Error.Create("Forecast.InvalidTimeRange", "Start time must be before end time.");

        public static Error TimeRangeTooLarge(int maxDays) =>
            Error.Create("Forecast.TimeRangeTooLarge", $"Time range cannot exceed {maxDays} days.");

        public static Error InvalidHourAlignment =>
            Error.Create("Forecast.InvalidHourAlignment", "Timestamps must be hour-aligned (minutes and seconds must be 0).");

        public static Error InvalidTimezone =>
            Error.Create("Forecast.InvalidTimezone", "Timestamps must be in UTC.");

        public static Error NegativeMwh =>
            Error.Create("Forecast.NegativeMwh", "MWh values must be non-negative.");

        public static Error NoDataPoints =>
            Error.Create("Forecast.NoDataPoints", "At least one forecast data point is required.");
    }

    public static class Position
    {
        public static Error CompanyNotFound(Guid companyId) =>
            Error.Create("Position.CompanyNotFound", $"Company with ID '{companyId}' was not found or is inactive.");

        public static Error InvalidTimeRange =>
            Error.Create("Position.InvalidTimeRange", "Start time must be before end time.");

        public static Error TimeRangeTooLarge(int maxDays) =>
            Error.Create("Position.TimeRangeTooLarge", $"Time range cannot exceed {maxDays} days.");

        public static Error InvalidHourAlignment =>
            Error.Create("Position.InvalidHourAlignment", "Timestamps must be hour-aligned (minutes and seconds must be 0).");

        public static Error InvalidTimezone =>
            Error.Create("Position.InvalidTimezone", "Timestamps must be in UTC.");
    }

    public static class PowerPlant
    {
        public static Error NotFound(Guid plantId) =>
            Error.Create("PowerPlant.NotFound", $"Power plant with ID '{plantId}' was not found.");
    }
}
