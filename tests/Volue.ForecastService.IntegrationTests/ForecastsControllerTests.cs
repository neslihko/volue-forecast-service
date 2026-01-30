using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Volue.ForecastService.Api.Models;
using Volue.ForecastService.Contracts.DTOs;
using Volue.ForecastService.Contracts.Models;
using Xunit;

namespace Volue.ForecastService.IntegrationTests;

/// <summary>
/// Integration tests for Forecast API endpoints.
/// Tests CREATE, UPDATE (idempotent), and GET operations.
/// </summary>
public class ForecastsControllerTests : IClassFixture<ForecastServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Guid _testPlantId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Istanbul Wind Farm
    private readonly Guid _invalidPlantId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public ForecastsControllerTests(ForecastServiceWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateForecast_WithValidData_Returns200AndInsertsCounts()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), Mwh = 100.5m },
                new() { HourUtc = new DateTime(2026, 2, 1, 11, 0, 0, DateTimeKind.Utc), Mwh = 120.0m },
                new() { HourUtc = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc), Mwh = 95.75m }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateOrUpdateForecastResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlantId.Should().Be(_testPlantId);
        result.Data.InsertedCount.Should().Be(3);
        result.Data.UpdatedCount.Should().Be(0);
        result.Data.UnchangedCount.Should().Be(0);
        result.Data.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateForecast_WithSameData_Returns200AndUnchangedCount()
    {
        // Arrange - First insert
        var request = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = new DateTime(2026, 2, 2, 10, 0, 0, DateTimeKind.Utc), Mwh = 150.0m }
            }
        };
        await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", request);

        // Act - Send same data again (idempotent)
        var response = await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateOrUpdateForecastResponse>>();
        result.Should().NotBeNull();
        result!.Data!.InsertedCount.Should().Be(0);
        result.Data.UpdatedCount.Should().Be(0);
        result.Data.UnchangedCount.Should().Be(1);
        result.Data.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateForecast_WithDifferentValue_Returns200AndUpdatedCount()
    {
        // Arrange - First insert
        var hour = new DateTime(2026, 2, 3, 10, 0, 0, DateTimeKind.Utc);
        var insertRequest = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = hour, Mwh = 100.0m }
            }
        };
        await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", insertRequest);

        // Act - Update with different value
        var updateRequest = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = hour, Mwh = 200.0m } // Different value
            }
        };
        var response = await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateOrUpdateForecastResponse>>();
        result.Should().NotBeNull();
        result!.Data!.InsertedCount.Should().Be(0);
        result.Data.UpdatedCount.Should().Be(1);
        result.Data.UnchangedCount.Should().Be(0);
        result.Data.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task CreateForecast_WithInvalidPlantId_Returns404()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), Mwh = 100.0m }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/forecasts/{_invalidPlantId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateForecast_WithNonHourAlignedTimestamp_Returns400()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = new DateTime(2026, 2, 1, 10, 30, 0, DateTimeKind.Utc), Mwh = 100.0m } // 30 minutes
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateForecast_WithNegativeMwh_Returns400()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), Mwh = -50.0m }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetForecast_WithValidRange_Returns200AndForecasts()
    {
        // Arrange - Insert test data
        var from = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 2, 5, 3, 0, 0, DateTimeKind.Utc);

        var insertRequest = new CreateOrUpdateForecastRequest
        {
            Forecasts = new List<ForecastPoint>
            {
                new() { HourUtc = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc), Mwh = 100.0m },
                new() { HourUtc = new DateTime(2026, 2, 5, 1, 0, 0, DateTimeKind.Utc), Mwh = 110.0m },
                new() { HourUtc = new DateTime(2026, 2, 5, 2, 0, 0, DateTimeKind.Utc), Mwh = 120.0m }
            }
        };
        await _client.PutAsJsonAsync($"/api/forecasts/{_testPlantId}", insertRequest);

        // Act
        var response = await _client.GetAsync($"/api/forecasts/{_testPlantId}?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GetForecastResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Forecasts.Should().HaveCount(3);
        result.Data.Forecasts.Should().BeInAscendingOrder(f => f.HourUtc);
    }

    [Fact]
    public async Task GetForecast_WithInvalidRange_Returns400()
    {
        // Arrange - from > to
        var from = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await _client.GetAsync($"/api/forecasts/{_testPlantId}?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetForecast_WithTooLargeRange_Returns400()
    {
        // Arrange - More than 7 days
        var from = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc); // 9 days

        // Act
        var response = await _client.GetAsync($"/api/forecasts/{_testPlantId}?from={from:O}&to={to:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CorrelationId_IsReturnedInResponse()
    {
        // Arrange
        var correlationId = "test-correlation-123";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/forecasts/{_testPlantId}?from=2026-02-01T00:00:00Z&to=2026-02-02T00:00:00Z");
        request.Headers.Add("X-Correlation-ID", correlationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
        response.Headers.GetValues("X-Correlation-ID").First().Should().Be(correlationId);
    }
}
