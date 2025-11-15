using FluentAssertions;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Core.Test.ClientGroupTests;

public class ClientGroupEdgeCaseTests
{
    [Theory]
    [InlineData("Group-Name")]        // hyphen
    [InlineData("Group_Name")]        // underscore
    [InlineData("Group 123")]         // space and numbers
    [InlineData("Ab")]                // minimum length (2)
    [InlineData("123456789-123456789-123456789-123456789-")] // 50 characters
    public void Create_WithValidNameBoundaries_ShouldNotThrow(string validName)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithName(validName)
            .Build();

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("A")]                            // 1 character (too short)
    [InlineData("#123456789-123456789-123456789-123456789-1")] // 51 characters (too long)
    public void Create_WithInvalidNameBoundaries_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithName(invalidName)
            .Build();

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_MultipleTimes_ShouldUpdateTimestampsCorrectly()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();
        var firstUpdateBy = "admin1";
        var secondUpdateBy = "admin2";

        // Act
        group.Update("First Update", "First description", firstUpdateBy);
        var firstUpdateTime = group.UpdatedAt;

        // Wait a moment to ensure different timestamps
        Thread.Sleep(10);

        group.Update("Second Update", "Second description", secondUpdateBy);
        var secondUpdateTime = group.UpdatedAt;

        // Assert
        firstUpdateTime.Should().NotBeNull();
        secondUpdateTime.Should().NotBeNull();
        secondUpdateTime.Should().BeAfter(firstUpdateTime.Value);
        group.UpdatedBy.Should().Be(secondUpdateBy);
    }
}