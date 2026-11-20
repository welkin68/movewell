using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Errors;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Domain.Tests.Aggregates.OtpChallenges;

public class OtpChallengeTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly MobileNumber Mobile = MobileNumber.Create("09123456789").Value;

    private static OtpChallenge Issue(int maxAttempts = 3) =>
        OtpChallenge.Issue(Mobile, OtpPurpose.Registration, "HASH", Now, TimeSpan.FromMinutes(2), maxAttempts).Value;

    private static string Code(FluentResults.Result result) => ((DomainError)result.Errors[0]).Code;

    [Fact]
    public void Issue_sets_expiry_from_lifetime()
    {
        var otp = Issue();

        Assert.Equal(Now.AddMinutes(2), otp.ExpiresAt);
        Assert.True(otp.IsOpen(Now));
    }

    [Fact]
    public void Issue_rejects_non_positive_settings()
    {
        Assert.True(OtpChallenge.Issue(Mobile, OtpPurpose.Registration, "H", Now, TimeSpan.Zero, 3).IsFailed);
        Assert.True(OtpChallenge.Issue(Mobile, OtpPurpose.Registration, "H", Now, TimeSpan.FromMinutes(2), 0).IsFailed);
    }

    [Fact]
    public void Verify_with_correct_hash_then_consume_succeeds()
    {
        var otp = Issue();

        Assert.True(otp.Verify("HASH", Now.AddSeconds(30)).IsSuccess);
        Assert.True(otp.Consume(Now.AddSeconds(31)).IsSuccess);
        Assert.NotNull(otp.ConsumedAt);
        Assert.False(otp.IsOpen(Now.AddSeconds(32)));
    }

    [Fact]
    public void Wrong_code_counts_attempts_and_locks_at_max()
    {
        var otp = Issue(maxAttempts: 2);

        Assert.Equal("otp.invalid", Code(otp.Verify("WRONG", Now)));
        Assert.Equal("otp.too_many_attempts", Code(otp.Verify("WRONG", Now)));
        Assert.Equal("otp.too_many_attempts", Code(otp.Verify("HASH", Now)));
        Assert.Equal(2, otp.Attempts);
    }

    [Fact]
    public void Expired_code_is_rejected_even_when_correct()
    {
        var otp = Issue();

        Assert.Equal("otp.expired", Code(otp.Verify("HASH", Now.AddMinutes(2))));
        Assert.Equal(0, otp.Attempts);
    }

    [Fact]
    public void Verified_code_cannot_be_verified_again()
    {
        var otp = Issue();
        otp.Verify("HASH", Now);

        Assert.Equal("otp.already_used", Code(otp.Verify("HASH", Now)));
    }

    [Fact]
    public void Consume_requires_verification_and_happens_once()
    {
        var otp = Issue();

        Assert.Equal("otp.not_verified", Code(otp.Consume(Now)));

        otp.Verify("HASH", Now);
        otp.Consume(Now);

        Assert.Equal("otp.already_used", Code(otp.Consume(Now)));
    }
}
