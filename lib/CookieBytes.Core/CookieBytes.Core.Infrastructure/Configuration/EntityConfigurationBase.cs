using CookieBytes.Core.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookieBytes.Core.Infrastructure.Configuration;

/// <summary>
/// Base EF configuration for entities inheriting from <see cref="Entity{TId}"/>.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class EntityConfigurationBase<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <inheritdoc />
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(entity => entity.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(entity => entity.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .HasColumnName("UpdatedAt");

        builder.Property(entity => entity.UpdatedBy)
            .HasColumnName("UpdatedBy")
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue("System");
    }
}

/// <summary>
/// Base EF configuration for Guid-keyed entities.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public abstract class EntityConfigurationBase<TEntity> : EntityConfigurationBase<TEntity, Guid>
    where TEntity : Entity<Guid>
{
}

/// <summary>
/// Base EF configuration for integer identity entities (database-generated keys).
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public abstract class IntEntityConfigurationBase<TEntity> : EntityConfigurationBase<TEntity, int>
    where TEntity : Entity<int>
{
    /// <inheritdoc />
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
    }
}
