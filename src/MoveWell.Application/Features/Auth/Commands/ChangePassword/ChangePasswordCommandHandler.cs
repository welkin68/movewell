using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.Features.Auth.Commands.Shared;

namespace MoveWell.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly ITokenService _tokens;

    public ChangePasswordCommandHandler(ICurrentUserService currentUser, IIdentityService identity, ITokenService tokens)
    {
        _currentUser = currentUser;
        _identity = identity;
        _tokens = tokens;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            return Result.Fail(AuthErrors.NotSignedIn);
        }

        var changed = await _identity.ChangePasswordAsync(userId, command.CurrentPassword, command.NewPassword, cancellationToken);
        if (changed.IsFailed)
        {
            return changed;
        }

        await _tokens.RevokeAllAsync(userId, cancellationToken);
        return Result.Ok();
    }
}
