using System.Security.Cryptography;
using MoveWell.Application.Abstractions.Security;

namespace MoveWell.Infrastructure.Security;

public sealed class OtpCodeGenerator : IOtpCodeGenerator
{
    public string Generate(int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 4);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, 8);

        var max = (int)Math.Pow(10, length);
        return RandomNumberGenerator.GetInt32(0, max).ToString().PadLeft(length, '0');
    }
}
