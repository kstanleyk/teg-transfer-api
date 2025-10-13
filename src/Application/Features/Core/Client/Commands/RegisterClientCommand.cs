using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Client.Dto;
using TegWallet.Application.Features.Core.Client.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Client.Commands;

public record RegisterClientCommand(
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string CurrencyCode) : IRequest<Result<ClientRegisteredDto>>;

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
            return Result<ClientRegisteredDto>.Failed("Client with this email already exists");

        // Validate currency code
        var currency = Currency.FromCode(command.CurrencyCode);

        // Create client (automatically creates wallet)
        var client = Domain.Entity.Core.Client.Create(command.Email.Trim().ToLower(), command.PhoneNumber.Trim(),
            command.FirstName.Trim(), command.LastName.Trim(), currency);

        // Save to database
        var result = await clientRepository.AddAsync(client);
        if (result.Status != RepositoryActionStatus.Created)
            return Result<ClientRegisteredDto>.Failed($"An error occured while creating client account");

        // Map to DTO and return
        var clientDto = mapper.Map<ClientRegisteredDto>(result.Entity);
        return Result<ClientRegisteredDto>.Succeeded(clientDto,"Client account created successfully.");
    }
}