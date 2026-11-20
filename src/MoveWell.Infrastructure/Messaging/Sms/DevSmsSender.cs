using FluentResults;
using Microsoft.Extensions.Logging;
using MoveWell.Application.Abstractions.Messaging;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Infrastructure.Messaging.Sms;

/// <summary>
/// Development only: writes the full SMS text (including the code) to the log so flows can be tested without an SMS account.
/// Registration refuses this provider outside Development/Testing.
/// </summary>
public sealed class DevSmsSender : ISmsSender
{
    private readonly ILogger<DevSmsSender> _logger;

    public DevSmsSender(ILogger<DevSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendOtpAsync(MobileNumber mobile, string code, OtpPurpose purpose, Culture culture, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("[DEV SMS] to {Mobile}: {Text}", mobile.Value, SmsTexts.Otp(code, purpose, culture));
        return Task.FromResult(Result.Ok());
    }
}
