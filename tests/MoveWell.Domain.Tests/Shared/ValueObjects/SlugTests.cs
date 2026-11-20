using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Domain.Tests.Shared.ValueObjects;

public class SlugTests
{
    [Theory]
    [InlineData("chin-tuck")]
    [InlineData("cat-cow-2")]
    [InlineData("abc")]
    [InlineData("  Wall-Angel  ")]
    public void Create_accepts_valid_slugs(string input)
    {
        var result = Slug.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(input.Trim().ToLowerInvariant(), result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ab")]
    [InlineData("-chin")]
    [InlineData("chin-")]
    [InlineData("chin--tuck")]
    [InlineData("chin tuck")]
    [InlineData("چانه")]
    public void Create_rejects_invalid_slugs(string? input)
    {
        Assert.True(Slug.Create(input).IsFailed);
    }

    [Fact]
    public void Create_rejects_too_long_slug()
    {
        Assert.True(Slug.Create(new string('a', Slug.MaxLength + 1)).IsFailed);
    }

    [Theory]
    [InlineData("Chin Tuck", "chin-tuck")]
    [InlineData("  Cat / Cow (Stretch)  ", "cat-cow-stretch")]
    [InlineData("90/90 Hip Switch", "90-90-hip-switch")]
    public void FromText_builds_slug_from_title(string title, string expected)
    {
        var result = Slug.FromText(title);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void FromText_fails_for_persian_only_text()
    {
        Assert.True(Slug.FromText("حرکت چانه").IsFailed);
    }
}
