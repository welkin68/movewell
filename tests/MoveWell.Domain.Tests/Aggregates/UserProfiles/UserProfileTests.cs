using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Domain.Shared.Enumerations;

namespace MoveWell.Domain.Tests.Aggregates.UserProfiles;

public class UserProfileTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_collapses_spaces_and_records_terms_acceptance()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "  سارا   احمدی ", Culture.Fa, Now).Value;

        Assert.Equal("سارا احمدی", profile.DisplayName);
        Assert.Equal(Now, profile.AcceptedTermsAt);
        Assert.Equal(Culture.Fa, profile.PreferredCulture);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("a")]
    public void Create_rejects_short_names(string? name)
    {
        Assert.True(UserProfile.Create(Guid.NewGuid(), name, Culture.Fa, Now).IsFailed);
    }

    [Fact]
    public void Create_rejects_long_names()
    {
        var name = new string('x', UserProfile.DisplayNameMaxLength + 1);

        Assert.True(UserProfile.Create(Guid.NewGuid(), name, Culture.Fa, Now).IsFailed);
    }

    [Fact]
    public void Create_requires_a_user()
    {
        Assert.True(UserProfile.Create(Guid.Empty, "Sara", Culture.Fa, Now).IsFailed);
    }

    [Fact]
    public void Rename_and_change_culture_update_state()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "Sara", Culture.Fa, Now).Value;

        Assert.True(profile.Rename("Sara Ahmadi").IsSuccess);
        Assert.True(profile.ChangeCulture(Culture.En).IsSuccess);
        Assert.Equal("Sara Ahmadi", profile.DisplayName);
        Assert.Equal(Culture.En, profile.PreferredCulture);
    }

    [Fact]
    public void Failed_rename_keeps_old_name()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "Sara", Culture.Fa, Now).Value;

        Assert.True(profile.Rename("x").IsFailed);
        Assert.Equal("Sara", profile.DisplayName);
    }
}
