using FluentResults;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.DTOs.Auth;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Features.Auth.Commands.SignIn;

public sealed class SignInCommandHandler : IRequestHandler<SignInCommand, Result<AuthSessionDto>>
{
    private readonly IMoveWellUnitOfWork _unitOfWork;
    private readonly IIdentityService _identity;
    private readonly ITokenService _tokens;

    public SignInCommandHandler(IMoveWellUnitOfWork unitOfWork, IIdentityService identity, ITokenService tokens)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _tokens = tokens;
    }

    public async Task<Result<AuthSessionDto>> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        // A malformed number gets the same answer as a wrong password: sign-in never says which part was wrong.
        var mobile = MobileNumber.Create(command.Mobile);
        if (mobile.IsFailed)
        {
            return Result.Fail<AuthSessionDto>(AuthErrors.InvalidCredentials);
        }

        var user = await _identity.CheckPasswordAsync(mobile.Value, command.Password, cancellationToken);
        if (user.IsFailed)
        {
            return user.ToResult<AuthSessionDto>();
        }

        var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(user.Value.UserId, cancellationToken);
        var displayName = profile?.DisplayName ?? mobile.Value.Masked;
        var culture = profile?.PreferredCulture ?? Culture.Default;

        return Result.Ok(await _tokens.IssueAsync(user.Value, displayName, culture, command.ClientIp, cancellationToken));
    }
}
