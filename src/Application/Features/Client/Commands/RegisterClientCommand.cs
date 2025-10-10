using AutoMapper;
using MediatR;
using Transfer.Application.Features.Client.Dto;
using Transfer.Application.Features.Client.Validators;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Core;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Client.Commands;

public record RegisterClientCommand(
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string? CurrencyCode) : IRequest<Result<ClientRegisteredDto>>;

public class RegisterClientCommandHandler(
    IClientRepository clientRepository,
    IMapper mapper) : IRequestHandler<RegisterClientCommand, Result<ClientRegisteredDto>>
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
            return Result<ClientRegisteredDto>.Failure("Client with this email already exists");

        // Validate currency code
        var currency = GetCurrencyFromCode(command.CurrencyCode);
        if (currency == null)
            return Result<ClientRegisteredDto>.Failure($"Unsupported currency code: {command.CurrencyCode}");

        // Create client (automatically creates wallet)
        var client = Domain.Entity.Core.Client.Create(command.Email.Trim().ToLower(), command.PhoneNumber.Trim(),
            command.FirstName.Trim(), command.LastName.Trim(), currency);

        // Save to database
        var result = await clientRepository.AddAsync(client);
        if (result.Status != RepositoryActionStatus.Created)
            return Result<ClientRegisteredDto>.Failure($"An error occured while creating client account");

        // Map to DTO and return
        var clientDto = mapper.Map<ClientRegisteredDto>(result.Entity);
        return Result<ClientRegisteredDto>.Success(clientDto);
    }

    private static Currency? GetCurrencyFromCode(string? code)
    {
        return code?.ToUpper() switch
        {
            "USD" => Currency.USD,
            "NGN" => Currency.NGN,
            "XOF" => Currency.XOF,
            _ => null
        };
    }
}