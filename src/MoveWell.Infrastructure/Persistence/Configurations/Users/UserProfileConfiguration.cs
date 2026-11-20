using CookieBytes.Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Infrastructure.Identity;

namespace MoveWell.Infrastructure.Persistence.Configurations.Users;

public sealed class UserProfileConfiguration : EntityConfigurationBase<UserProfile>
{
    public override void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserProfiles");

        builder.Property(profile => profile.UserId).IsRequired();
        builder.HasIndex(profile => profile.UserId).IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(profile => profile.DisplayName)
            .HasMaxLength(UserProfile.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(profile => profile.PreferredCulture)
            .HasConversion(EnumerationConverter.Create<Culture>())
            .HasColumnName("PreferredCultureId")
            .IsRequired();

        builder.Property(profile => profile.AcceptedTermsAt).IsRequired();
    }
}
