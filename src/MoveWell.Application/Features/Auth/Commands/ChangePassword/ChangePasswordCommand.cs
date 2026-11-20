using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.Features.Auth.Commands.Shared;

namespace MoveWell.Application.Features.Auth.Commands.ChangePassword;

/// <summary>Signed-in user changes the password; all refresh tokens are revoked (other devices must sign in again).</summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result>;
