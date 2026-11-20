using CookieBytes.Core.Domain.SeedWork;

namespace MoveWell.Domain.Shared.Enumerations;

/// <summary>Lifecycle of publishable content. Only <see cref="Published"/> is visible to the public.</summary>
public sealed class PublishState : Enumeration
{
    public static readonly PublishState Draft = new(1, nameof(Draft));
    public static readonly PublishState Published = new(2, nameof(Published));
    public static readonly PublishState Archived = new(3, nameof(Archived));

    private PublishState(int value, string name)
        : base(value, name)
    {
    }
}
