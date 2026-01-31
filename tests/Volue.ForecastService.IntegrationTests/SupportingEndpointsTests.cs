using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Volue.ForecastService.Api.Models;
using Volue.ForecastService.Contracts.Models;
using Xunit;

namespace Volue.ForecastService.IntegrationTests;

/// <summary>
/// Integration tests for Power Plants and Health Check endpoints.
/// </summary>
public class SupportingEndpointsTests : IClassFixture<ForecastServiceWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SupportingEndpointsTests(ForecastServiceWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllPowerPlants_Returns200AndListOfPlants()
    {
        // Act
        var response = await _client.GetAsync("/api/power-plants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PowerPlantInfo>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(3); // From seed data
        result.Data.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetPowerPlantById_WithValidId_Returns200AndPlantDetails()
    {
        // Arrange
        var plantId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Istanbul Wind Farm

        // Act
        var response = await _client.GetAsync($"/api/power-plants/{plantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PowerPlantInfo>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(plantId);
        result.Data.Name.Should().Be("Istanbul Wind Farm");
        result.Data.Country.Should().Be("Turkey");
    }

    [Fact]
    public async Task GetPowerPlantById_WithInvalidId_Returns404()
    {
        // Arrange
        var invalidPlantId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var response = await _client.GetAsync($"/api/power-plants/{invalidPlantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthLive_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthReady_WithDatabaseConnected_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
