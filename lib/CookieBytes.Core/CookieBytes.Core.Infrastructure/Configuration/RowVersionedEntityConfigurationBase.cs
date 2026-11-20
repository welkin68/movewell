using CookieBytes.Core.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookieBytes.Core.Infrastructure.Configuration;

/// <summary>
/// Base EF configuration for entities inheriting from <see cref="RowVersionedEntity"/>.
/// </summary>
public abstract class RowVersionedEntityConfigurationBase<TEntity> : EntityConfigurationBase<TEntity>
    where TEntity : RowVersionedEntity
{
    /// <inheritdoc />
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.RowVersion)
            .HasColumnName("RowVersion")
            .IsRowVersion();
    }
}
