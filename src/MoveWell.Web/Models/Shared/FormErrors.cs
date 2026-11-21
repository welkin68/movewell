namespace MoveWell.Web.Models.Shared;

/// <summary>Messages shown at the top of a form (already localized by the Api or Web resources).</summary>
public sealed class FormErrors
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    public bool Any => _messages.Count > 0;

    public void Add(string message)
    {
        if (!string.IsNullOrWhiteSpace(message) && !_messages.Contains(message))
        {
            _messages.Add(message);
        }
    }

    public void AddRange(IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            Add(message);
        }
    }
}
