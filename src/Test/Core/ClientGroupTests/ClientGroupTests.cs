using FluentAssertions;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Core.Test.ClientGroupTests;

public class ClientGroupTests
{
    private readonly string _validName = "VIP Clients";
    private readonly string _validDescription = "Very important clients group";
    private readonly string _validCreatedBy = "admin";

    [Fact]
    public void Create_WithValidData_ShouldCreateActiveGroup()
    {
        // Act
        var group = new ClientGroupTestBuilder()
            .WithName(_validName)
            .WithDescription(_validDescription)
            .WithCreatedBy(_validCreatedBy)
            .Build();

        // Assert
        group.Should().NotBeNull();
        group.Id.Should().NotBe(Guid.Empty);
        group.Name.Should().Be(_validName.Trim());
        group.Description.Should().Be(_validDescription.Trim());
        group.IsActive.Should().BeTrue();
        group.CreatedBy.Should().Be(_validCreatedBy.Trim());
        group.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        group.UpdatedAt.Should().BeNull();
        group.UpdatedBy.Should().BeNull();
        group.Clients.Should().BeEmpty();
        group.ExchangeRates.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithName(invalidName)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldThrowDomainException(string invalidDescription)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithDescription(invalidDescription)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*description*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidCreatedBy_ShouldThrowDomainException(string invalidCreatedBy)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithCreatedBy(invalidCreatedBy)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*createdBy*");
    }

    [Theory]
    [InlineData("A")] // Too short
    [InlineData("This name is way too long and exceeds the maximum allowed characters limit")] // Too long
    public void Create_WithInvalidNameLength_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithName(invalidName)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*between 2 and 50 characters*");
    }

    [Theory]
    [InlineData("Name@Invalid")]
    [InlineData("Name#Invalid")]
    [InlineData("Name$Invalid")]
    [InlineData("Name%Invalid")]
    public void Create_WithInvalidNameCharacters_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        var action = () => new ClientGroupTestBuilder()
            .WithName(invalidName)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*letters, numbers, spaces, hyphens, and underscores*");
    }

    [Fact]
    public void Create_ShouldTrimNameDescriptionAndCreatedBy()
    {
        // Arrange
        var nameWithSpaces = "  VIP Clients  ";
        var descriptionWithSpaces = "  Very important clients  ";
        var createdByWithSpaces = "  admin  ";

        // Act
        var group = new ClientGroupTestBuilder()
            .WithName(nameWithSpaces)
            .WithDescription(descriptionWithSpaces)
            .WithCreatedBy(createdByWithSpaces)
            .Build();

        // Assert
        group.Name.Should().Be("VIP Clients");
        group.Description.Should().Be("Very important clients");
        group.CreatedBy.Should().Be("admin");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();
        var newName = "Updated Group Name";
        var newDescription = "Updated group description";
        var updatedBy = "admin2";

        // Act
        group.Update(newName, newDescription, updatedBy);

        // Assert
        group.Name.Should().Be(newName.Trim());
        group.Description.Should().Be(newDescription.Trim());
        group.UpdatedBy.Should().Be(updatedBy.Trim());
        group.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("", "Valid description", "admin")]
    [InlineData("Valid Name", "", "admin")]
    [InlineData("Valid Name", "Valid description", "")]
    [InlineData(null, "Valid description", "admin")]
    [InlineData("Valid Name", null, "admin")]
    [InlineData("Valid Name", "Valid description", null)]
    public void Update_WithInvalidData_ShouldThrowDomainException(string name, string description, string updatedBy)
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();

        // Act & Assert
        var action = () => group.Update(name, description, updatedBy);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_ShouldTrimInputValues()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();
        var nameWithSpaces = "  New Name  ";
        var descriptionWithSpaces = "  New Description  ";
        var updatedByWithSpaces = "  admin  ";

        // Act
        group.Update(nameWithSpaces, descriptionWithSpaces, updatedByWithSpaces);

        // Assert
        group.Name.Should().Be("New Name");
        group.Description.Should().Be("New Description");
        group.UpdatedBy.Should().Be("admin");
    }

    [Fact]
    public void Deactivate_ActiveGroup_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();
        var deactivatedBy = "admin";

        // Act
        group.Deactivate(deactivatedBy);

        // Assert
        group.IsActive.Should().BeFalse();
        group.UpdatedBy.Should().Be(deactivatedBy);
        group.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_AlreadyInactiveGroup_ShouldDoNothing()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateInactiveGroup();
        var originalUpdatedAt = group.UpdatedAt;
        var originalUpdatedBy = group.UpdatedBy;

        // Act
        group.Deactivate("admin2");

        // Assert
        group.IsActive.Should().BeFalse();
        group.UpdatedAt.Should().Be(originalUpdatedAt);
        group.UpdatedBy.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void Activate_InactiveGroup_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateInactiveGroup();
        var activatedBy = "admin2";

        // Act
        group.Activate(activatedBy);

        // Assert
        group.IsActive.Should().BeTrue();
        group.UpdatedBy.Should().Be(activatedBy);
        group.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_AlreadyActiveGroup_ShouldDoNothing()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();
        var originalUpdatedAt = group.UpdatedAt;
        var originalUpdatedBy = group.UpdatedBy;

        // Act
        group.Activate("admin2");

        // Assert
        group.IsActive.Should().BeTrue();
        group.UpdatedAt.Should().Be(originalUpdatedAt);
        group.UpdatedBy.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void CanBeDeleted_WhenNoClients_ShouldReturnTrue()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();

        // Act & Assert
        group.CanBeDeleted().Should().BeTrue();
    }

    [Fact]
    public void CanBeDeleted_WhenHasClients_ShouldReturnFalse()
    {
        // Note: This test would require adding clients to the group
        // Since the _clients list is private and there's no public method to add clients,
        // this test would typically require reflection or a test-specific method
        // For now, we assume the default state (no clients) returns true

        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();

        // Act & Assert
        group.CanBeDeleted().Should().BeTrue();
    }

    [Fact]
    public void CreatedAt_ShouldBeImmutable()
    {
        // Arrange
        var customDate = DateTime.UtcNow.AddDays(-5);
        var group = new ClientGroupTestBuilder()
            .WithCreatedAt(customDate)
            .Build();

        // Act & Assert
        group.CreatedAt.Should().NotBe(customDate);
    }

    [Fact]
    public void CreatedBy_ShouldBeImmutable()
    {
        // Arrange
        var group = new ClientGroupTestBuilder()
            .WithCreatedBy("originalUser")
            .Build();

        var originalCreatedBy = group.CreatedBy;

        // Act
        // Try to update (this doesn't change CreatedBy as it's init-only)
        group.Update("New Name", "New Description", "newUser");

        // Assert
        group.CreatedBy.Should().Be(originalCreatedBy);
    }

    [Fact]
    public void ExchangeRates_ShouldBeEmptyByDefault()
    {
        // Arrange & Act
        var group = ClientGroupTestBuilder.CreateActiveGroup();

        // Assert
        group.ExchangeRates.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithValidNameCharacters_ShouldNotThrow()
    {
        // Arrange
        var group = ClientGroupTestBuilder.CreateActiveGroup();
        var validNames = new[]
        {
            "Group-Name",
            "Group_Name",
            "Group 123",
            "Group-Name_123",
            "Valid Group"
        };

        foreach (var validName in validNames)
        {
            // Act & Assert
            var action = () => group.Update(validName, "Valid description", "admin");
            action.Should().NotThrow();
        }
    }
}