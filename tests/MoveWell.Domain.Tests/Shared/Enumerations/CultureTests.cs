using MoveWell.Domain.Shared.Enumerations;

namespace MoveWell.Domain.Tests.Shared.Enumerations;

public class CultureTests
{
    [Theory]
    [InlineData("fa", "fa")]
    [InlineData("fa-IR", "fa")]
    [InlineData("EN", "en")]
    [InlineData("en-US", "en")]
    [InlineData("de-DE", "fa")]
    [InlineData(null, "fa")]
    [InlineData("", "fa")]
    public void FromCode_maps_codes_and_falls_back_to_persian(string? code, string expected)
    {
        Assert.Equal(expected, Culture.FromCode(code).Name);
    }
}
