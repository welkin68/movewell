using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Infrastructure.Persistence.Converters;

/// <summary>Single-value VOs stored as one column. Rows come from our own writes, so invalid data means corruption → throw.</summary>
public static class ValueObjectConverters
{
    public static ValueConverter<MobileNumber, string> Mobile { get; } = new(
        mobile => mobile.Value,
        value => ToMobile(value));

    public static ValueConverter<Slug, string> Slug { get; } = new(
        slug => slug.Value,
        value => ToSlug(value));

    private static MobileNumber ToMobile(string value) =>
        MobileNumber.Create(value).ValueOrDefault ?? throw Corrupt(nameof(MobileNumber), value);

    private static Slug ToSlug(string value) =>
        Domain.Shared.ValueObjects.Slug.Create(value).ValueOrDefault ?? throw Corrupt(nameof(Slug), value);

    private static InvalidOperationException Corrupt(string type, string value) =>
        new($"Stored {type} value '{value}' is invalid.");
}
