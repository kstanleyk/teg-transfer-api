using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Core.Test.ClientTests;

public class ClientTestBuilder
{
    private string _email = "test@example.com";
    private string _phoneNumber = "+1234567890";
    private string _firstName = "Test";
    private string _lastName = "User";
    private Currency? _defaultCurrency;
    private ClientGroup? _clientGroup;
    private DateTime? _createdAt;
    private ClientStatus? _desiredStatus;
    private Guid? _userId;
    private string _suspensionReason = "Test suspension";
    private string _deactivationReason = "Test deactivation";

    public ClientTestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ClientTestBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public ClientTestBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public ClientTestBuilder WithDefaultCurrency(Currency currency)
    {
        _defaultCurrency = currency;
        return this;
    }

    public ClientTestBuilder WithClientGroup(ClientGroup clientGroup)
    {
        _clientGroup = clientGroup;
        return this;
    }

    public ClientTestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ClientTestBuilder WithStatus(ClientStatus status)
    {
        _desiredStatus = status;
        return this;
    }

    public ClientTestBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public ClientTestBuilder WithSuspensionReason(string reason)
    {
        _suspensionReason = reason;
        return this;
    }

    public ClientTestBuilder WithDeactivationReason(string reason)
    {
        _deactivationReason = reason;
        return this;
    }

    public Client Build()
    {
        var client = Client.Create(
            _email,
            _phoneNumber,
            _firstName,
            _lastName,
            _defaultCurrency,
            _clientGroup,
            _createdAt
        );

        // Apply status if specified (different from default Active)
        if (_desiredStatus.HasValue && _desiredStatus.Value != ClientStatus.Active)
        {
            switch (_desiredStatus.Value)
            {
                case ClientStatus.Inactive:
                    client.Deactivate(_deactivationReason);
                    break;
                case ClientStatus.Suspended:
                    client.Suspend(_suspensionReason);
                    break;
            }
        }

        // Link user if specified
        if (_userId.HasValue)
        {
            client.LinkToUser(_userId.Value);
        }

        return client;
    }

    // Predefined factory methods for common scenarios

    public static Client CreateValidClient() => new ClientTestBuilder().Build();

    public static Client CreateClientWithGroup(ClientGroup group) =>
        new ClientTestBuilder().WithClientGroup(group).Build();

    public static Client CreateSuspendedClient() =>
        new ClientTestBuilder().WithStatus(ClientStatus.Suspended).Build();

    public static Client CreateSuspendedClient(string reason) =>
        new ClientTestBuilder()
            .WithStatus(ClientStatus.Suspended)
            .WithSuspensionReason(reason)
            .Build();

    public static Client CreateInactiveClient() =>
        new ClientTestBuilder().WithStatus(ClientStatus.Inactive).Build();

    public static Client CreateInactiveClient(string reason) =>
        new ClientTestBuilder()
            .WithStatus(ClientStatus.Inactive)
            .WithDeactivationReason(reason)
            .Build();

    public static Client CreateCorporateClient() =>
        new ClientTestBuilder()
            .WithEmail("corporate@company.com")
            .WithName("Corporate", "Client")
            .WithDefaultCurrency(Currency.USD)
            .Build();

    public static Client CreateClientWithCurrency(Currency currency) =>
        new ClientTestBuilder()
            .WithDefaultCurrency(currency)
            .Build();

    public static Client CreateHistoricalClient(DateTime creationDate) =>
        new ClientTestBuilder()
            .WithCreatedAt(creationDate)
            .Build();

    public static Client CreateClientWithUser(Guid userId) =>
        new ClientTestBuilder()
            .WithUserId(userId)
            .Build();

    public static Client CreateClientWithSpacesInData() =>
        new ClientTestBuilder()
            .WithEmail("  spaced@email.com  ")
            .WithPhoneNumber("  +1234567890  ")
            .WithName("  John  ", "  Doe  ")
            .Build();

    public static Client CreateVipClient(ClientGroup vipGroup) =>
        new ClientTestBuilder()
            .WithEmail("vip@client.com")
            .WithName("VIP", "Client")
            .WithDefaultCurrency(Currency.XAF)
            .WithClientGroup(vipGroup)
            .Build();

    // Method to create multiple clients for bulk operations testing
    public static List<Client> CreateMultipleClients(int count, Action<ClientTestBuilder, int>? configure = null)
    {
        var clients = new List<Client>();
        for (int i = 0; i < count; i++)
        {
            var builder = new ClientTestBuilder()
                .WithEmail($"client{i}@example.com")
                .WithPhoneNumber($"+123456789{i}")
                .WithName($"Client{i}", "Test");

            configure?.Invoke(builder, i);
            clients.Add(builder.Build());
        }
        return clients;
    }
}