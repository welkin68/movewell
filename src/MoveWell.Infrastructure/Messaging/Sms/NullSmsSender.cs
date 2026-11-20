using FluentResults;
using Microsoft.Extensions.Logging;
using MoveWell.Application.Abstractions.Messaging;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Infrastructure.Messaging.Sms;

/// <summary>Sends nothing. Default until a real provider is configured (Q-12); never logs the code.</summary>
public sealed class NullSmsSender : ISmsSender
{
    private readonly ILogger<NullSmsSender> _logger;

    public NullSmsSender(ILogger<NullSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendOtpAsync(MobileNumber mobile, string code, OtpPurpose purpose, Culture culture, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SMS provider is Null: OTP for {Mobile} ({Purpose}) was not sent.", mobile.Masked, purpose.Name);
        return Task.FromResult(Result.Ok());
    }
}
