namespace Feedy.Domain.Common;

/// <summary>
/// Unit type for operations that succeed or fail but return no data.
/// Use as Result&lt;Nothing&gt; — e.g. delete, logout, send-email.
/// </summary>
public sealed record Nothing
{
    public static readonly Nothing Value = new();
    private Nothing() { }
}
