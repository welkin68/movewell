using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MoveWell.Application.Abstractions.Security;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;
using MoveWell.Infrastructure.Options;

namespace MoveWell.Infrastructure.Security;

/// <summary>HMAC-SHA256(secret, "purpose|mobile|code") as hex. Without the secret a stolen table is useless.</summary>
public sealed class OtpHasher : IOtpHasher
{
    private readonly byte[] _key;

    public OtpHasher(IOptions<OtpSecurityOptions> options)
    {
        _key = Encoding.UTF8.GetBytes(options.Value.HashSecret);
    }

    public string Hash(string code, MobileNumber mobile, OtpPurpose purpose)
    {
        var payload = Encoding.UTF8.GetBytes($"{purpose.Value}|{mobile.Value}|{code.Trim()}");
        return Convert.ToHexString(HMACSHA256.HashData(_key, payload));
    }
}
