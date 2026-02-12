using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Clients.Dto;
using TegWallet.Application.Features.Core.Clients.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Core.Clients.Command;

public record RegisterClientCommand(
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string Password,
    string CurrencyCode) : IRequest<Result<ClientRegisteredDto>>;

public class RegisterClientCommandHandler(
    IClientRepository clientRepository,
    IMapper mapper) : RequestHandlerBase, IRequestHandler<RegisterClientCommand, Result<ClientRegisteredDto>>
{
    public async Task<Result<ClientRegisteredDto>> Handle(RegisterClientCommand command, CancellationToken cancellationToken)
    {
        var validator = new RegisterClientCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        // Check if client already exists
        var existingClient = await clientRepository.GetByEmailAsync(command.Email);
        if (existingClient != null)
            return Result<ClientRegisteredDto>.Failed("Client with this email already exists");

        // Validate currency code
        var currency = Domain.ValueObjects.Currency.FromCode(command.CurrencyCode);

        var parameters = new RegisterClientParameters(
            command.FirstName.Trim(),
            command.LastName.Trim(),
            command.Email.Trim().ToLower(),
            command.PhoneNumber.Trim(),
            command.Password,
            "System");

        // Save to database
        var createResult = await clientRepository.RegisterClientAsync(parameters);

        if (createResult.Status != RepositoryActionStatus.Created)
            return Result<ClientRegisteredDto>.Failed($"An error occured while creating client account");

        // Map to DTO and return
        var clientDto = mapper.Map<ClientRegisteredDto>(createResult.Entity);
        return Result<ClientRegisteredDto>.Succeeded(clientDto,"Client account created successfully.");
    }


}

public record RegisterClientParameters
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;

    public RegisterClientParameters(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string password,
        string createdBy)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Password = password;
        CreatedBy = createdBy;
    }

    public void Validate()
    {
        DomainGuards.AgainstNullOrWhiteSpace(FirstName, nameof(FirstName));
        DomainGuards.AgainstNullOrWhiteSpace(LastName, nameof(LastName));
        DomainGuards.AgainstNullOrWhiteSpace(Email, nameof(Email));
        DomainGuards.AgainstNullOrWhiteSpace(PhoneNumber, nameof(PhoneNumber));
        DomainGuards.AgainstNullOrWhiteSpace(Password, nameof(Password));
        DomainGuards.AgainstNullOrWhiteSpace(CreatedBy, nameof(CreatedBy));

        if (Password.Length < 6)
            throw new DomainException("Password must be at least 6 characters long");

        if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("Invalid email format");

        if (!System.Text.RegularExpressions.Regex.IsMatch(PhoneNumber, @"^\+?[\d\s-]{10,}$"))
            throw new DomainException("Invalid phone number format");
    }
}