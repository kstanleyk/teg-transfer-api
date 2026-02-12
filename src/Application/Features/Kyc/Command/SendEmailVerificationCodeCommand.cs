using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TegWallet.Application.Features.Kyc.Validator;
using TegWallet.Application.Helpers;
using TegWallet.Application.Helpers.Exceptions;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.Domain.Entity.Kyc;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Application.Features.Kyc.Command;

public record SendEmailVerificationCodeCommand(Guid ClientId) : IRequest<Result>;

public class SendEmailVerificationCodeCommandHandler : IRequestHandler<SendEmailVerificationCodeCommand, Result>
{
    private readonly IKycProfileRepository _kycProfileRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppLocalizer _localizer;
    private readonly ILogger<SendEmailVerificationCodeCommandHandler> _logger;

    public SendEmailVerificationCodeCommandHandler(
        IKycProfileRepository kycProfileRepository,
        IClientRepository clientRepository,
        IAppLocalizer localizer,
        ILogger<SendEmailVerificationCodeCommandHandler> logger)
    {
        _kycProfileRepository = kycProfileRepository;
        _clientRepository = clientRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(SendEmailVerificationCodeCommand command, CancellationToken cancellationToken)
    {
        var validator = new SendEmailVerificationCodeCommandValidator();
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
            // Get client
            var client = await _clientRepository.GetAsync(command.ClientId);
            if (client == null)
            {
                return Result.Failed($"Client with ID {command.ClientId} not found.");
            }

            // Get KYC profile
            var kycProfile = await _kycProfileRepository.GetKycProfileByClientIdWithEmailVerificationAsync(command.ClientId);
            if (kycProfile == null)
            {
                return Result.Failed($"KYC profile not found for client ID {command.ClientId}.");
            }

            // Business logic validation in handler
            if (kycProfile.EmailVerification == null)
            {
                return Result.Failed("Email verification not initialized for this client.");
            }

            if (kycProfile.EmailVerification.IsVerified)
            {
                var alreadyVerifiedMessage = _localizer["EmailAlreadyVerified"];
                return Result.Succeeded(alreadyVerifiedMessage);
            }

            // Check rate limiting in application layer
            var recentAttempts = kycProfile.EmailVerification.Attempts
                .Where(a => a.AttemptedAt > DateTime.UtcNow.AddMinutes(-15))
                .Count(a => a.Successful);

            if (recentAttempts >= 3)
            {
                return Result.Failed("Too many verification attempts. Please try again later.");
            }

            // Generate verification code
            var verificationCode = GenerateVerificationCode();

            //// Send email
            //var emailSent = await _emailVerificationService.SendVerificationCodeAsync(
            //    client.Email,
            //    verificationCode,
            //    client.FullName);

            //if (!emailSent)
            //{
            //    return Result.Failed("Failed to send verification email. Please try again.");
            //}

            // Update domain entity
            kycProfile.EmailVerification.MarkAsSubmitted();

            // Add verification attempt with the generated code
            // Note: This requires the domain entity to have a method to add attempts
            AddVerificationAttempt(kycProfile.EmailVerification, verificationCode);

            // Save changes
            //await _kycProfileRepository.UpdateAsync(kycProfile);
            //await _unitOfWork.SaveChangesAsync();
            //await transaction.CommitAsync();

            // Cache verification code (optional, service layer concern)
            await CacheVerificationCode(client.Email, verificationCode);

            var successMessage = _localizer["EmailVerificationCodeSent"];
            return Result.Succeeded(successMessage);
        }
        catch (DomainException ex)
        {
            return Result.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification code for client {ClientId}", command.ClientId);
            return Result.Failed("An error occurred while sending the verification code. Please try again.");
        }
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private void AddVerificationAttempt(EmailVerification emailVerification, string verificationCode)
    {
        // Since Attempts is a private collection, we need to use reflection or add a domain method
        // Better approach: Add a domain method to EmailVerification entity
        // For now, using reflection as an example
        var attemptsField = typeof(EmailVerification)
            .GetField("_attempts", BindingFlags.NonPublic | BindingFlags.Instance);

        if (attemptsField?.GetValue(emailVerification) is List<KycVerificationAttempt> attempts)
        {
            attempts.Add(new KycVerificationAttempt(
                DateTime.UtcNow,
                KycVerificationMethod.SelfService,
                true,
                referenceId: verificationCode));
        }
    }

    private async Task CacheVerificationCode(string email, string code)
    {
        // Implementation depends on your caching strategy
        // Example with IMemoryCache:
        // _cache.Set($"email_verification_{email}", code, TimeSpan.FromMinutes(15));
    }
}

public record SendEmailVerificationCodeParameters(Guid ClientId);

