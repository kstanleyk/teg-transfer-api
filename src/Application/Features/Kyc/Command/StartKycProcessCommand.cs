using MediatR;
using TegWallet.Application.Features.Kyc.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Kyc.Command;

public record StartKycProcessCommand(Guid ClientId) : IRequest<Result<Guid>>;

public class StartKycProcessCommandHandler : IRequestHandler<StartKycProcessCommand, Result<Guid>>
{
    private readonly IKycProfileRepository _kycRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppLocalizer _localizer;

    public StartKycProcessCommandHandler(
        IKycProfileRepository kycRepository,
        IClientRepository clientRepository,
        IAppLocalizer localizer)
    {
        _kycRepository = kycRepository;
        _clientRepository = clientRepository;
        _localizer = localizer;
    }

    public async Task<Result<Guid>> Handle(StartKycProcessCommand command, CancellationToken cancellationToken)
    {
        var validator = new StartKycProcessCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(validationErrors);
        }

        try
        {
            // First, check if client exists
            var client = await _clientRepository.GetAsync(command.ClientId);
            if (client == null)
            {
                return Result<Guid>.Failed($"Client with ID {command.ClientId} not found.");
            }

            // Check if KYC profile already exists
            var existingProfile = await _kycRepository.GetKycProfileByClientIdAsync(command.ClientId);
            if (existingProfile != null)
            {
                // If KYC already exists, return the existing profile ID
                var message = _localizer["KycProfileAlreadyExists"];
                return Result<Guid>.Succeeded(existingProfile.Id, message);
            }

            var parameters = new StartKycProcessParameters(command.ClientId);
            var result = await _kycRepository.StartKycProcessAsync(parameters);

            if (result.Status != RepositoryActionStatus.Created)
            {
                return Result<Guid>.Failed(
                    "An unexpected error occurred while starting the KYC process. Please try again.");
            }

            var successMessage = _localizer["KycProcessStartedSuccess"];
            return Result<Guid>.Succeeded(result.Entity!.Id, successMessage);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failed($"Failed to start KYC process: {ex.Message}");
        }
    }
}

public record StartKycProcessParameters(Guid ClientId);

