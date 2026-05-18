namespace Feedy.Domain.Common;

/// <summary>
/// Discriminated union representing either a successful value of type T or a domain Error.
/// Use Nothing as T for operations that return no data on success.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(Error error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    /// <summary>The success value. Throws if the result is a failure.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    /// <summary>The domain error. Throws if the result is a success.</summary>
    public Error Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(Error error) => new(error);
}
