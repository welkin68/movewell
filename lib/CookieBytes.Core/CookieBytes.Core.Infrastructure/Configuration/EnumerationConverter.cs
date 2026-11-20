using CookieBytes.Core.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CookieBytes.Core.Infrastructure.Configuration;

/// <summary>
/// EF value converter helpers for smart enumerations.
/// </summary>
public static class EnumerationConverter
{
    /// <summary>
    /// Creates an EF Core value converter that persists enumerations as integers.
    /// </summary>
    /// <typeparam name="TEnumeration">The enumeration type.</typeparam>
    public static ValueConverter<TEnumeration, int> Create<TEnumeration>()
        where TEnumeration : Enumeration
    {
        return new ValueConverter<TEnumeration, int>(
            enumeration => enumeration.Value,
            value => ConvertFromValue<TEnumeration>(value));
    }

    /// <summary>
    /// Creates an EF Core value converter that persists nullable enumerations as nullable integers.
    /// </summary>
    /// <typeparam name="TEnumeration">The enumeration type.</typeparam>
    public static ValueConverter<TEnumeration?, int?> CreateNullable<TEnumeration>()
        where TEnumeration : Enumeration
    {
        return new ValueConverter<TEnumeration?, int?>(
            enumeration => enumeration == null ? null : enumeration.Value,
            value => value == null ? null : ConvertFromValue<TEnumeration>(value.Value));
    }

    private static TEnumeration ConvertFromValue<TEnumeration>(int value)
        where TEnumeration : Enumeration
    {
        return Enumeration.FromValue<TEnumeration>(value)
            ?? throw new InvalidOperationException($"Unknown {typeof(TEnumeration).Name} value: {value}");
    }
}
