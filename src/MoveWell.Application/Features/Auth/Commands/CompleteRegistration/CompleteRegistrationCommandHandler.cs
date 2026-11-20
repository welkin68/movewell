using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.DTOs.Auth;
using MoveWell.Application.Services.Otp;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Features.Auth.Commands.CompleteRegistration;

/// <remarks>
/// Order matters: everything that can fail without side effects (password policy, name, existing account) is checked
/// BEFORE the code is verified, so a typo in the password never burns a valid code.
/// Account + profile + OTP consumption are committed in one transaction.
/// </remarks>
public sealed class CompleteRegistrationCommandHandler : IRequestHandler<CompleteRegistrationCommand, Result<AuthSessionDto>>
{
    private readonly IMoveWellUnitOfWork _unitOfWork;
    private readonly IIdentityService _identity;
    private readonly ITokenService _tokens;
    private readonly OtpService _otp;
    private readonly IDateTimeProvider _clock;

    public CompleteRegistrationCommandHandler(
        IMoveWellUnitOfWork unitOfWork,
        IIdentityService identity,
        ITokenService tokens,
        OtpService otp,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _tokens = tokens;
        _otp = otp;
        _clock = clock;
    }

    public async Task<Result<AuthSessionDto>> Handle(CompleteRegistrationCommand command, CancellationToken cancellationToken)
    {
        var mobile = MobileNumber.Create(command.Mobile);
        if (mobile.IsFailed)
        {
            return mobile.ToResult<AuthSessionDto>();
        }

        var culture = Culture.FromCode(command.Culture);

        var password = await _identity.ValidatePasswordAsync(command.Password, cancellationToken);
        if (password.IsFailed)
        {
            return password.ToResult<AuthSessionDto>();
        }

        // Validates the name with the real domain rule before any side effect (the id is replaced below).
        var nameCheck = UserProfile.Create(Guid.NewGuid(), command.DisplayName, culture, _clock.UtcNow);
        if (nameCheck.IsFailed)
        {
            return nameCheck.ToResult<AuthSessionDto>();
        }

        if (await _identity.MobileExistsAsync(mobile.Value, cancellationToken))
        {
            return Result.Fail<AuthSessionDto>(AuthErrors.AlreadyRegistered);
        }

        var challenge = await _otp.VerifyAsync(mobile.Value, OtpPurpose.Registration, command.Code, cancellationToken);
        if (challenge.IsFailed)
        {
            return challenge.ToResult<AuthSessionDto>();
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _identity.CreateMemberAsync(mobile.Value, command.Password, cancellationToken);
            if (user.IsFailed)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return user.ToResult<AuthSessionDto>();
            }

            var profile = UserProfile.Create(user.Value.UserId, command.DisplayName, culture, _clock.UtcNow).Value;
            await _unitOfWork.UserProfiles.AddAsync(profile, cancellationToken);
            challenge.Value.Consume(_clock.UtcNow);

            await _unitOfWork.SaveAndDispatchEventsAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            var session = await _tokens.IssueAsync(user.Value, profile.DisplayName, culture, command.ClientIp, cancellationToken);
            return Result.Ok(session);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
