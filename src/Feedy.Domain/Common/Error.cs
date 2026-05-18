namespace Feedy.Domain.Common;

/// <summary>
/// Domain error with a machine-readable code and a human-readable message.
/// Returned by application services via Result&lt;T&gt; instead of throwing exceptions.
/// </summary>
public record Error(string Code, string Message);
