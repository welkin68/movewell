using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.Errors;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Domain.Tests.Shared.ValueObjects;

public class LocalizedTextTests
{
    [Fact]
    public void Create_trims_both_languages()
    {
        var text = LocalizedText.Create("  چانه به داخل  ", " Chin tuck ", 50, required: true).Value;

        Assert.Equal("چانه به داخل", text.Fa);
        Assert.Equal("Chin tuck", text.En);
        Assert.True(text.IsComplete);
    }

    [Fact]
    public void Create_requires_persian_when_required()
    {
        var result = LocalizedText.Create("  ", "Chin tuck", 50, required: true);

        Assert.True(result.IsFailed);
        Assert.Equal("text.fa_required", ((DomainError)result.Errors[0]).Code);
    }

    [Fact]
    public void Create_allows_empty_when_optional()
    {
        var text = LocalizedText.Create(null, null, 50, required: false).Value;

        Assert.True(text.IsEmpty);
        Assert.False(text.IsComplete);
    }

    [Fact]
    public void Create_rejects_text_longer_than_max_in_either_language()
    {
        Assert.True(LocalizedText.Create("abc", new string('x', 11), 10, required: true).IsFailed);
        Assert.True(LocalizedText.Create(new string('x', 11), "abc", 10, required: true).IsFailed);
    }

    [Fact]
    public void English_missing_is_not_complete_and_falls_back_to_persian()
    {
        var text = LocalizedText.Create("کشش گردن", "", 50, required: true).Value;

        Assert.False(text.IsComplete);
        Assert.Equal("کشش گردن", text.For(Culture.En));
        Assert.Equal("کشش گردن", text.For(Culture.Fa));
    }

    [Fact]
    public void For_returns_requested_language()
    {
        var text = LocalizedText.Create("کشش گردن", "Neck stretch", 50, required: true).Value;

        Assert.Equal("Neck stretch", text.For(Culture.En));
        Assert.Equal("کشش گردن", text.For(Culture.Fa));
    }

    [Fact]
    public void Same_texts_are_equal()
    {
        Assert.Equal(
            LocalizedText.Create("الف", "A", 10, true).Value,
            LocalizedText.Create(" الف ", "A ", 10, true).Value);
    }
}
