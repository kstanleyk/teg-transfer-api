using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Features.Core.Client.Dto;
using TegWallet.Application.Features.Core.Client.Validators;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Client.Commands;

public record RegisterClientCommand(
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string Password,
    string CurrencyCode) : IRequest<Result<ClientRegisteredDto>>;

public class RegisterClientCommandHandler(
    UserManager<Domain.Entity.Core.Client> userManager,
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
        var existingClient = await userManager.FindByEmailAsync(command.Email);
        if (existingClient != null)
            return Result<ClientRegisteredDto>.Failed("Client with this email already exists");

        // Validate currency code
        var currency = Domain.ValueObjects.Currency.FromCode(command.CurrencyCode);

        // Create client (automatically creates wallet)
        var client = Domain.Entity.Core.Client.Create(command.Email.Trim().ToLower(), command.PhoneNumber.Trim(),
            command.FirstName.Trim(), command.LastName.Trim(), currency);

        // Save to database
        var createResult = await userManager.CreateAsync(client, command.Password);

        if (!createResult.Succeeded)
        {
            var identityErrors = createResult.Errors.Select(e => e.Description).ToList();
            throw new ValidationException(identityErrors);
        }

        //var result = await userManager.AddAsync(client);
        //if (result.Status != RepositoryActionStatus.Created)
        //    return Result<ClientRegisteredDto>.Failed($"An error occured while creating client account");

        // Map to DTO and return
        var clientDto = mapper.Map<ClientRegisteredDto>(client);
        return Result<ClientRegisteredDto>.Succeeded(clientDto,"Client account created successfully.");
    }

    //protected override void DisposeCore()
    //{
    //    userManager.Dispose();
    //}
}