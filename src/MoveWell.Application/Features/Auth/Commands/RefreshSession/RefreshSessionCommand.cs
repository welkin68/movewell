using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.DTOs.Auth;

namespace MoveWell.Application.Features.Auth.Commands.RefreshSession;

/// <summary>Exchanges a refresh token for a new access + refresh token pair (rotation).</summary>
public sealed record RefreshSessionCommand(string RefreshToken, string? ClientIp) : IRequest<Result<AuthSessionDto>>;
