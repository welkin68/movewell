namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Optional concurrency marker for optimistic concurrency tokens.
/// </summary>
public interface IRowVersioned
{
    byte[] RowVersion { get; }

    void SetRowVersion(byte[] rowVersion);
}
