using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoveWell.Infrastructure.Identity;

namespace MoveWell.Infrastructure.Persistence.Configurations.Identity;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.HasIndex(token => token.TokenHash).IsUnique();

        builder.HasIndex(token => new { token.UserId, token.RevokedAt });
        builder.HasIndex(token => token.FamilyId);

        builder.Property(token => token.CreatedByIp).HasMaxLength(45).IsUnicode(false);
        builder.Property(token => token.RevokedReason).HasMaxLength(32).IsUnicode(false);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
