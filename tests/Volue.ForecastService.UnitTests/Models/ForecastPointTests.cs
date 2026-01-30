using FluentAssertions;
using Volue.ForecastService.Contracts.Models;
using Xunit;

namespace Volue.ForecastService.UnitTests.Models;

/// <summary>
/// Unit tests for the ForecastPoint model.
/// </summary>
public class ForecastPointTests
{
    [Fact]
    public void ForecastPoint_ShouldInitializeCorrectly()
    {
        // Arrange
        var hourUtc = new DateTime(2026, 1, 30, 10, 0, 0, DateTimeKind.Utc);
        var mwh = 150.5m;

        // Act
        var point = new ForecastPoint
        {
            HourUtc = hourUtc,
            Mwh = mwh
        };

        // Assert
        point.HourUtc.Should().Be(hourUtc);
        point.Mwh.Should().Be(mwh);
        point.HourUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ForecastPoint_ShouldBeRecord()
    {
        // Arrange
        var hourUtc = new DateTime(2026, 1, 30, 10, 0, 0, DateTimeKind.Utc);
        var point1 = new ForecastPoint { HourUtc = hourUtc, Mwh = 100m };
        var point2 = new ForecastPoint { HourUtc = hourUtc, Mwh = 100m };
        var point3 = new ForecastPoint { HourUtc = hourUtc, Mwh = 200m };

        // Act & Assert
        point1.Should().Be(point2); // Records with same values should be equal
        point1.Should().NotBe(point3); // Records with different values should not be equal
    }
}
