using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;

namespace MoveWell.Application.Features.Auth.Commands.SignOut;

/// <summary>Revokes the given refresh token. The short-lived access token simply expires.</summary>
public sealed record SignOutCommand(string RefreshToken) : IRequest<Result>;
