using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Volue.ForecastService.Api.Models;
using Volue.ForecastService.Contracts.DTOs;
using Volue.ForecastService.Contracts.Models;
using Xunit;

namespace Volue.ForecastService.IntegrationTests;

/// <summary>
/// Integration tests for Company Position API endpoint.
/// Tests aggregation of forecasts across multiple power plants.
/// </summary>
public class PositionsControllerTests : IClassFixture<ForecastServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Guid _companyId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Energy Trading Corp
    private readonly Guid _plant1Id = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Istanbul
    private readonly Guid _plant2Id = Guid.Parse("33333333-3333-3333-3333-333333333333"); // Sofia
    private readonly Guid _plant3Id = Guid.Parse("44444444-4444-4444-4444-444444444444"); // Madrid

    public PositionsControllerTests(ForecastServiceWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCompanyPosition_WithMultiplePlants_Returns200AndAggregatedData()
    {
        // Arrange - Insert forecasts for multiple plants
        var hour1 = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var hour2 = new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc);

        // Plant 1: 100 MWh at 10:00, 150 MWh at 11:00
        await _client.PutAsJsonAsync($"/api/forecasts/{_plant1Id}", new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = hour1, Mwh = 100.0m },
                new() { HourUtc = hour2, Mwh = 150.0m }
            }
        });

        // Plant 2: 50 MWh at 10:00, 75 MWh at 11:00
        await _client.PutAsJsonAsync($"/api/forecasts/{_plant2Id}", new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = hour1, Mwh = 50.0m },
                new() { HourUtc = hour2, Mwh = 75.0m }
            }
        });

        // Plant 3: 30 MWh at 10:00, 45 MWh at 11:00
        await _client.PutAsJsonAsync($"/api/forecasts/{_plant3Id}", new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = hour1, Mwh = 30.0m },
                new() { HourUtc = hour2, Mwh = 45.0m }
            }
        });

        // Act
        var from = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var response = await _client.GetAsync($"/api/company/{_companyId}/position?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CompanyPosition>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CompanyId.Should().Be(_companyId);
        result.Data.Positions.Should().HaveCount(2);

        // Verify aggregation: 10:00 → 100 + 50 + 30 = 180 MWh
        var position1 = result.Data.Positions.FirstOrDefault(p => p.HourUtc == hour1);
        position1.Should().NotBeNull();
        position1!.TotalMwh.Should().Be(180.0m);

        // Verify aggregation: 11:00 → 150 + 75 + 45 = 270 MWh
        var position2 = result.Data.Positions.FirstOrDefault(p => p.HourUtc == hour2);
        position2.Should().NotBeNull();
        position2!.TotalMwh.Should().Be(270.0m);
    }

    [Fact]
    public async Task GetCompanyPosition_WithNoData_Returns200AndEmptyList()
    {
        // Arrange
        var from = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Future date with no data
        var to = new DateTime(2030, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await _client.GetAsync($"/api/company/{_companyId}/position?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CompanyPosition>>();
        result.Should().NotBeNull();
        result!.Data!.Positions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompanyPosition_WithInvalidCompanyId_Returns404()
    {
        // Arrange
        var invalidCompanyId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var from = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await _client.GetAsync($"/api/company/{invalidCompanyId}/position?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCompanyPosition_WithInvalidTimeRange_Returns400()
    {
        // Arrange - from > to
        var from = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await _client.GetAsync($"/api/company/{_companyId}/position?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
