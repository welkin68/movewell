using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Domain.Tests.Shared.ValueObjects;

public class MobileNumberTests
{
    [Theory]
    [InlineData("09123456789")]
    [InlineData("0912 345 6789")]
    [InlineData("0912-345-6789")]
    [InlineData("+989123456789")]
    [InlineData("00989123456789")]
    [InlineData("989123456789")]
    [InlineData("9123456789")]
    [InlineData("۰۹۱۲۳۴۵۶۷۸۹")]
    [InlineData("٠٩١٢٣٤٥٦٧٨٩")]
    [InlineData(" (0912) 345 6789 ")]
    public void Create_normalizes_valid_inputs(string input)
    {
        var result = MobileNumber.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("09123456789", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0912345678")]
    [InlineData("091234567890")]
    [InlineData("02112345678")]
    [InlineData("09123a56789")]
    [InlineData("0912+3456789")]
    public void Create_rejects_invalid_inputs(string? input)
    {
        var result = MobileNumber.Create(input);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Masked_hides_the_middle_digits()
    {
        var mobile = MobileNumber.Create("09123456789").Value;

        Assert.Equal("0912***6789", mobile.Masked);
    }

    [Fact]
    public void Equal_numbers_in_different_formats_are_equal()
    {
        Assert.Equal(MobileNumber.Create("+989123456789").Value, MobileNumber.Create("۰۹۱۲۳۴۵۶۷۸۹").Value);
    }
}
