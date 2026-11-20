using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Abstractions.Security;

/// <summary>Keyed hash of a code bound to its mobile and purpose, so a leaked table cannot be replayed elsewhere.</summary>
public interface IOtpHasher
{
    string Hash(string code, MobileNumber mobile, OtpPurpose purpose);
}
