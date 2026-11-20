using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;

namespace MoveWell.Application.Features.Auth.Commands.SignOut;

public sealed class SignOutCommandHandler : IRequestHandler<SignOutCommand, Result>
{
    private readonly ITokenService _tokens;

    public SignOutCommandHandler(ITokenService tokens)
    {
        _tokens = tokens;
    }

    public async Task<Result> Handle(SignOutCommand command, CancellationToken cancellationToken)
    {
        await _tokens.RevokeAsync(command.RefreshToken, cancellationToken);
        return Result.Ok();
    }
}
