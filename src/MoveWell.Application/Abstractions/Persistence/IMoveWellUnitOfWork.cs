using CookieBytes.Core.Domain.Interfaces;
using MoveWell.Domain.Repositories.Otp;
using MoveWell.Domain.Repositories.Users;

namespace MoveWell.Application.Abstractions.Persistence;

/// <summary>The only persistence entry point for handlers (never DbContext). One instance per request.</summary>
public interface IMoveWellUnitOfWork : IUnitOfWork
{
    IUserProfileRepository UserProfiles { get; }

    IOtpChallengeRepository OtpChallenges { get; }
}
