using FluentAssertions;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Core.Test.ClientTests;

public class ClientTests
{
    private readonly string _validEmail = "john.doe@example.com";
    private readonly string _validPhone = "+1234567890";
    private readonly string _validFirstName = "John";
    private readonly string _validLastName = "Doe";

    [Fact]
    public void Create_WithValidData_ShouldCreateClientWithWallet()
    {
        // Act
        var client = new ClientTestBuilder()
            .WithEmail(_validEmail)
            .WithPhoneNumber(_validPhone)
            .WithName(_validFirstName, _validLastName)
            .Build();

        // Assert
        client.Should().NotBeNull();
        client.Id.Should().NotBe(Guid.Empty);
        client.Email.Should().Be(_validEmail.Trim());
        client.PhoneNumber.Should().Be(_validPhone.Trim());
        client.FirstName.Should().Be(_validFirstName.Trim());
        client.LastName.Should().Be(_validLastName.Trim());
        client.Status.Should().Be(ClientStatus.Active);
        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        client.Wallet.Should().NotBeNull();
        client.Wallet.BaseCurrency.Should().Be(Currency.XAF);
        client.Wallet.Balance.Amount.Should().Be(0);
        client.Wallet.AvailableBalance.Amount.Should().Be(0);
        client.UserId.Should().BeNull();
        client.User.Should().BeNull();
    }

    [Fact]
    public void Create_WithDefaultCurrency_ShouldCreateClientWithSpecifiedCurrency()
    {
        // Arrange
        var currency = Currency.USD;

        // Act
        var client = new ClientTestBuilder()
            .WithDefaultCurrency(currency)
            .Build();

        // Assert
        client.Wallet.BaseCurrency.Should().Be(currency);
    }

    [Fact]
    public void Create_WithClientGroup_ShouldAssignToGroup()
    {
        // Arrange
        var clientGroup = ClientGroup.Create("VIP Clients", "VIP client group", "system");

        // Act
        var client = new ClientTestBuilder()
            .WithClientGroup(clientGroup)
            .Build();

        // Assert
        client.ClientGroupId.Should().Be(clientGroup.Id);
        client.ClientGroup.Should().Be(clientGroup);
    }

    [Fact]
    public void Create_WithCustomCreatedAt_ShouldUseProvidedDateTime()
    {
        // Arrange
        var customCreatedAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var client = new ClientTestBuilder()
            .WithCreatedAt(customCreatedAt)
            .Build();

        // Assert
        client.CreatedAt.Should().Be(customCreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string invalidEmail)
    {
        // Act & Assert
        var action = () => new ClientTestBuilder()
            .WithEmail(invalidEmail)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("missing@domain")]
    [InlineData("@missingusername.com")]
    public void Create_WithMalformedEmail_ShouldThrowDomainException(string malformedEmail)
    {
        // Act & Assert
        var action = () => new ClientTestBuilder()
            .WithEmail(malformedEmail)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("Invalid email format");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidPhoneNumber_ShouldThrowDomainException(string invalidPhone)
    {
        // Act & Assert
        var action = () => new ClientTestBuilder()
            .WithPhoneNumber(invalidPhone)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*phoneNumber*");
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("abc")] // No digits
    [InlineData("+12-345-678")] // Not enough digits after cleaning
    public void Create_WithInvalidPhoneFormat_ShouldThrowDomainException(string invalidPhone)
    {
        // Act & Assert
        var action = () => new ClientTestBuilder()
            .WithPhoneNumber(invalidPhone)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*phone*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidFirstName_ShouldThrowDomainException(string invalidFirstName)
    {
        // Act & Assert
        var action = () => new ClientTestBuilder()
            .WithName(invalidFirstName, _validLastName)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*firstName*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidLastName_ShouldThrowDomainException(string invalidLastName)
    {
        // Act & Assert
        var action = () => new ClientTestBuilder()
            .WithName(_validFirstName, invalidLastName)
            .Build();

        action.Should().Throw<DomainException>().WithMessage("*lastName*");
    }

    [Fact]
    public void Create_ShouldTrimEmailAndPhoneNumber()
    {
        // Arrange
        var emailWithSpaces = "  john.doe@example.com  ";
        var phoneWithSpaces = "  +1234567890  ";

        // Act
        var client = new ClientTestBuilder()
            .WithEmail(emailWithSpaces)
            .WithPhoneNumber(phoneWithSpaces)
            .Build();

        // Assert
        client.Email.Should().Be("john.doe@example.com");
        client.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact]
    public void FullName_ShouldReturnCorrectFormat()
    {
        // Arrange
        var client = new ClientTestBuilder()
            .WithName("John", "Doe")
            .Build();

        // Act & Assert
        client.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void UpdateContactInfo_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var newEmail = "new.email@example.com";
        var newPhone = "+9876543210";

        // Act
        var action = () => client.UpdateContactInfo(newEmail, newPhone);

        // Assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("", "+9876543210")]
    [InlineData("new.email@example.com", "")]
    [InlineData("invalid-email", "+9876543210")]
    [InlineData("new.email@example.com", "123")]
    public void UpdateContactInfo_WithInvalidData_ShouldThrowDomainException(string newEmail, string newPhone)
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();

        // Act & Assert
        var action = () => client.UpdateContactInfo(newEmail, newPhone);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignToGroup_WithValidGroup_ShouldAssignClient()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var clientGroup = ClientGroup.Create("VIP", "VIP clients", "system");

        // Act
        client.AssignToGroup(clientGroup);

        // Assert
        client.ClientGroupId.Should().Be(clientGroup.Id);
    }

    [Fact]
    public void AssignToGroup_WithInactiveGroup_ShouldThrowDomainException()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var clientGroup = ClientGroup.Create("Inactive", "Inactive group", "system");
        clientGroup.Deactivate("system");

        // Act & Assert
        var action = () => client.AssignToGroup(clientGroup);
        action.Should().Throw<DomainException>().WithMessage("*inactive*");
    }

    [Fact]
    public void AssignToGroup_WhenClientSuspended_ShouldThrowDomainException()
    {
        // Arrange
        var client = ClientTestBuilder.CreateSuspendedClient();
        var clientGroup = ClientGroup.Create("VIP", "VIP clients", "system");

        // Act & Assert
        var action = () => client.AssignToGroup(clientGroup);
        action.Should().Throw<DomainException>().WithMessage("*suspended*");
    }

    [Fact]
    public void AssignToGroup_WhenAlreadyInSameGroup_ShouldDoNothing()
    {
        // Arrange
        var clientGroup = ClientGroup.Create("VIP", "VIP clients", "system");
        var client = ClientTestBuilder.CreateClientWithGroup(clientGroup);

        // Act
        client.AssignToGroup(clientGroup);

        // Assert
        client.ClientGroupId.Should().Be(clientGroup.Id);
    }

    [Fact]
    public void RemoveFromGroup_WhenInGroup_ShouldRemoveClientFromGroup()
    {
        // Arrange
        var clientGroup = ClientGroup.Create("VIP", "VIP clients", "system");
        var client = ClientTestBuilder.CreateClientWithGroup(clientGroup);

        // Act
        client.RemoveFromGroup();

        // Assert
        client.ClientGroupId.Should().BeNull();
        client.ClientGroup.Should().BeNull();
    }

    [Fact]
    public void RemoveFromGroup_WhenNotInGroup_ShouldDoNothing()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();

        // Act
        client.RemoveFromGroup();

        // Assert
        client.ClientGroupId.Should().BeNull();
    }

    [Fact]
    public void UpdateGroup_WithNewGroup_ShouldAssignToNewGroup()
    {
        // Arrange
        var oldGroup = ClientGroup.Create("Old", "Old group", "system");
        var newGroup = ClientGroup.Create("New", "New group", "system");
        var client = ClientTestBuilder.CreateClientWithGroup(oldGroup);

        // Act
        client.UpdateGroup(newGroup);

        // Assert
        client.ClientGroupId.Should().Be(newGroup.Id);
    }

    [Fact]
    public void UpdateGroup_WithNull_ShouldRemoveFromGroup()
    {
        // Arrange
        var oldGroup = ClientGroup.Create("Old", "Old group", "system");
        var client = ClientTestBuilder.CreateClientWithGroup(oldGroup);

        // Act
        client.UpdateGroup(null);

        // Assert
        client.ClientGroupId.Should().BeNull();
        client.ClientGroup.Should().BeNull();
    }

    [Fact]
    public void Suspend_ActiveClient_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var reason = "Suspicious activity";

        // Act
        client.Suspend(reason);

        // Assert
        client.Status.Should().Be(ClientStatus.Suspended);
    }

    [Fact]
    public void Suspend_AlreadySuspendedClient_ShouldDoNothing()
    {
        // Arrange
        var client = ClientTestBuilder.CreateSuspendedClient();

        // Act
        client.Suspend("Second suspension");

        // Assert
        client.Status.Should().Be(ClientStatus.Suspended);
    }

    [Fact]
    public void Activate_SuspendedClient_ShouldChangeStatusToActive()
    {
        // Arrange
        var client = ClientTestBuilder.CreateSuspendedClient();

        // Act
        client.Activate();

        // Assert
        client.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public void Activate_InactiveClient_ShouldChangeStatusToActive()
    {
        // Arrange
        var client = ClientTestBuilder.CreateInactiveClient();

        // Act
        client.Activate();

        // Assert
        client.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public void Activate_ActiveClient_ShouldDoNothing()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();

        // Act
        client.Activate();

        // Assert
        client.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public void Deactivate_ActiveClient_ShouldChangeStatusToInactive()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();

        // Act
        client.Deactivate();

        // Assert
        client.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public void Deactivate_InactiveClient_ShouldDoNothing()
    {
        // Arrange
        var client = ClientTestBuilder.CreateInactiveClient();

        // Act
        client.Deactivate();

        // Assert
        client.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public void LinkToUser_WithValidUserId_ShouldSetUserId()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var userId = Guid.NewGuid();

        // Act
        client.LinkToUser(userId);

        // Assert
        client.UserId.Should().Be(userId);
    }

    [Fact]
    public void LinkToUser_WithDefaultUserId_ShouldThrowDomainException()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var defaultUserId = Guid.Empty;

        // Act & Assert
        var action = () => client.LinkToUser(defaultUserId);
        action.Should().Throw<DomainException>().WithMessage("*user*");
    }

    [Fact]
    public void HasChanges_WithDifferentProperties_ShouldReturnTrue()
    {
        // Arrange
        var client1 = new ClientTestBuilder().WithName("John", "Doe").Build();
        var client2 = new ClientTestBuilder().WithName("Jane", "Doe").Build();

        // Act
        var hasChanges = client1.HasChanges(client2);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_WithDifferentStatus_ShouldReturnTrue()
    {
        // Arrange
        var client1 = ClientTestBuilder.CreateValidClient();
        var client2 = ClientTestBuilder.CreateSuspendedClient();

        // Act
        var hasChanges = client1.HasChanges(client2);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_WithDifferentGroup_ShouldReturnTrue()
    {
        // Arrange
        var client1 = ClientTestBuilder.CreateValidClient();
        var client2 = ClientTestBuilder.CreateValidClient();
        var group = ClientGroup.Create("Group", "Test group", "system");
        client2.AssignToGroup(group);

        // Act
        var hasChanges = client1.HasChanges(client2);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_WithSameProperties_ShouldReturnFalse()
    {
        // Arrange
        var client1 = ClientTestBuilder.CreateValidClient();
        var client2 = ClientTestBuilder.CreateValidClient();

        // Act
        var hasChanges = client1.HasChanges(client2);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChanges_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();

        // Act
        var hasChanges = client.HasChanges(null);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChanges_WithSameReference_ShouldReturnFalse()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();

        // Act
        var hasChanges = client.HasChanges(client);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Wallet_Creation_ShouldHaveCorrectClientId()
    {
        // Act
        var client = ClientTestBuilder.CreateValidClient();

        // Assert
        client.Wallet.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public void Wallet_Creation_ShouldHaveZeroBalances()
    {
        // Act
        var client = ClientTestBuilder.CreateValidClient();

        // Assert
        client.Wallet.Balance.Amount.Should().Be(0);
        client.Wallet.AvailableBalance.Amount.Should().Be(0);
    }

    [Fact]
    public void Email_And_PhoneNumber_ShouldBeImmutableAfterCreation()
    {
        // Arrange
        var client = ClientTestBuilder.CreateValidClient();
        var originalEmail = client.Email;
        var originalPhone = client.PhoneNumber;

        // Act
        client.UpdateContactInfo("new@email.com", "+9876543210");

        // Assert
        client.Email.Should().Be(originalEmail);
        client.PhoneNumber.Should().Be(originalPhone);
    }

    [Fact]
    public void CreatedAt_ShouldBeImmutable()
    {
        // Arrange
        var customDate = DateTime.UtcNow.AddDays(-5);
        var client = new ClientTestBuilder()
            .WithCreatedAt(customDate)
            .Build();

        // Act & Assert
        client.CreatedAt.Should().Be(customDate);
    }
}