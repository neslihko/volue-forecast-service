using FluentAssertions;
using Volue.ForecastService.Contracts.Models;
using Xunit;

namespace Volue.ForecastService.UnitTests.Models;

/// <summary>
/// Unit tests for the UpsertResult model.
/// </summary>
public class UpsertResultTests
{
    [Fact]
    public void TotalChanged_ShouldReturnSumOfInsertedAndUpdated()
    {
        // Arrange
        var result = new UpsertResult
        {
            InsertedCount = 5,
            UpdatedCount = 3,
            UnchangedCount = 2
        };

        // Act
        var totalChanged = result.TotalChanged;

        // Assert
        totalChanged.Should().Be(8);
    }

    [Fact]
    public void HasChanges_ShouldReturnTrue_WhenThereAreChanges()
    {
        // Arrange
        var result = new UpsertResult
        {
            InsertedCount = 1,
            UpdatedCount = 0,
            UnchangedCount = 0
        };

        // Act & Assert
        result.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_ShouldReturnFalse_WhenNoChanges()
    {
        // Arrange
        var result = new UpsertResult
        {
            InsertedCount = 0,
            UpdatedCount = 0,
            UnchangedCount = 5
        };

        // Act & Assert
        result.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void TotalProcessed_ShouldReturnSumOfAllCounts()
    {
        // Arrange
        var result = new UpsertResult
        {
            InsertedCount = 5,
            UpdatedCount = 3,
            UnchangedCount = 2
        };

        // Act
        var totalProcessed = result.TotalProcessed;

        // Assert
        totalProcessed.Should().Be(10);
    }

    [Theory]
    [InlineData(5, 0, 0, true)]
    [InlineData(0, 3, 0, true)]
    [InlineData(2, 2, 1, true)]
    [InlineData(0, 0, 10, false)]
    public void HasChanges_ShouldWorkCorrectly(int inserted, int updated, int unchanged, bool expectedHasChanges)
    {
        // Arrange
        var result = new UpsertResult
        {
            InsertedCount = inserted,
            UpdatedCount = updated,
            UnchangedCount = unchanged
        };

        // Act & Assert
        result.HasChanges.Should().Be(expectedHasChanges);
    }
}
