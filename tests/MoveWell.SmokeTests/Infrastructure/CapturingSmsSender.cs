using System.Collections.Concurrent;
using FluentResults;
using MoveWell.Application.Abstractions.Messaging;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.SmokeTests.Infrastructure;

/// <summary>Replaces the SMS adapter in tests: remembers the last code sent to each mobile.</summary>
public sealed class CapturingSmsSender : ISmsSender
{
    private readonly ConcurrentDictionary<string, string> _lastCodes = new();

    public Task<Result> SendOtpAsync(MobileNumber mobile, string code, OtpPurpose purpose, Culture culture, CancellationToken cancellationToken = default)
    {
        _lastCodes[Key(mobile.Value, purpose)] = code;
        return Task.FromResult(Result.Ok());
    }

    public string? LastCode(string mobile, OtpPurpose purpose) =>
        _lastCodes.TryGetValue(Key(MobileNumber.Create(mobile).Value.Value, purpose), out var code) ? code : null;

    private static string Key(string mobile, OtpPurpose purpose) => $"{purpose.Name}:{mobile}";
}
