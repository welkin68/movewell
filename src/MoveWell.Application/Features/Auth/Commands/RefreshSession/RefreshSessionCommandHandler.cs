using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.DTOs.Auth;

namespace MoveWell.Application.Features.Auth.Commands.RefreshSession;

public sealed class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, Result<AuthSessionDto>>
{
    private readonly ITokenService _tokens;

    public RefreshSessionCommandHandler(ITokenService tokens)
    {
        _tokens = tokens;
    }

    public Task<Result<AuthSessionDto>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken) =>
        _tokens.RefreshAsync(command.RefreshToken, command.ClientIp, cancellationToken);
}
