using FluentResults;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Abstractions.Messaging;

/// <summary>SMS port. Adapters: Null, Dev (writes the code to the log), real provider later (Q-12).</summary>
public interface ISmsSender
{
    Task<Result> SendOtpAsync(
        MobileNumber mobile,
        string code,
        OtpPurpose purpose,
        Culture culture,
        CancellationToken cancellationToken = default);
}
