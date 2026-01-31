using FluentAssertions;
using Volue.ForecastService.Contracts.Models;
using Xunit;

namespace Volue.ForecastService.UnitTests.Models;

/// <summary>
/// Unit tests for the CompanyPosition model.
/// </summary>
public class CompanyPositionTests
{
    [Fact]
    public void TotalMwh_ShouldReturnSumOfAllPositions()
    {
        // Arrange
        var position = new CompanyPosition
        {
            CompanyId = Guid.NewGuid(),
            CompanyName = "Test Company",
            Positions = new List<HourlyPosition>
            {
                new() { HourUtc = DateTime.UtcNow, TotalMwh = 100.5m, PlantCount = 2 },
                new() { HourUtc = DateTime.UtcNow.AddHours(1), TotalMwh = 150.3m, PlantCount = 3 },
                new() { HourUtc = DateTime.UtcNow.AddHours(2), TotalMwh = 200.2m, PlantCount = 2 }
            }
        };

        // Act
        var totalMwh = position.TotalMwh;

        // Assert
        totalMwh.Should().Be(451.0m);
    }

    [Fact]
    public void TotalMwh_ShouldReturnZero_WhenNoPositions()
    {
        // Arrange
        var position = new CompanyPosition
        {
            CompanyId = Guid.NewGuid(),
            CompanyName = "Test Company",
            Positions = Array.Empty<HourlyPosition>()
        };

        // Act
        var totalMwh = position.TotalMwh;

        // Assert
        totalMwh.Should().Be(0);
    }

    [Fact]
    public void CompanyPosition_ShouldInitializeWithDefaults()
    {
        // Act
        var position = new CompanyPosition();

        // Assert
        position.CompanyId.Should().Be(Guid.Empty);
        position.CompanyName.Should().BeEmpty();
        position.Positions.Should().BeEmpty();
        position.TotalMwh.Should().Be(0);
    }
}
