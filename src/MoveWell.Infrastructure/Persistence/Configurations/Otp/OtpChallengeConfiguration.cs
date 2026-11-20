using CookieBytes.Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;
using MoveWell.Infrastructure.Persistence.Converters;

namespace MoveWell.Infrastructure.Persistence.Configurations.Otp;

public sealed class OtpChallengeConfiguration : EntityConfigurationBase<OtpChallenge>
{
    public override void Configure(EntityTypeBuilder<OtpChallenge> builder)
    {
        base.Configure(builder);

        builder.ToTable("OtpChallenges");

        builder.Property(otp => otp.Mobile)
            .HasConversion(ValueObjectConverters.Mobile)
            .HasMaxLength(MobileNumber.Length)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(otp => otp.Purpose)
            .HasConversion(EnumerationConverter.Create<OtpPurpose>())
            .HasColumnName("PurposeId")
            .IsRequired();

        builder.Property(otp => otp.CodeHash)
            .HasMaxLength(128)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(otp => otp.ExpiresAt).IsRequired();
        builder.Property(otp => otp.Attempts).IsRequired();
        builder.Property(otp => otp.MaxAttempts).IsRequired();

        builder.HasIndex(otp => new { otp.Mobile, otp.Purpose, otp.CreatedAt });
    }
}
