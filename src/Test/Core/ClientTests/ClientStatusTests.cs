using FluentAssertions;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Core.Test.ClientTests;

public class ClientStatusTests
{
    [Fact]
    public void ClientStatus_ShouldHaveExpectedMembers()
    {
        // Arrange & Act
        var values = Enum.GetValues<ClientStatus>();

        // Assert
        values.Should().HaveCount(3);
        values.Should().Contain([
            ClientStatus.Active,
            ClientStatus.Suspended,
            ClientStatus.Inactive
        ]);
    }

    [Fact]
    public void ClientStatus_ShouldHaveCorrectOrderForBusinessLogic()
    {
        // This test documents the expected priority/order if it matters for business logic
        var statuses = Enum.GetValues<ClientStatus>().ToList();

        statuses.IndexOf(ClientStatus.Active).Should().Be(0);
        statuses.IndexOf(ClientStatus.Suspended).Should().Be(1);
        statuses.IndexOf(ClientStatus.Inactive).Should().Be(2);
    }
}