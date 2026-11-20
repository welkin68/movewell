using FluentResults;
using MediatR;
using MoveWell.Application.DTOs.Auth;

namespace MoveWell.Application.Features.Auth.Commands.SignIn;

public sealed record SignInCommand(string Mobile, string Password, string? ClientIp) : IRequest<Result<AuthSessionDto>>;
