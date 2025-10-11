namespace TegWallet.Application.Features.Core.Client.Dto;

public record RegisterClientDto
{
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = "XOF"; // Default currency
}