namespace Volue.ForecastService.Contracts.Common;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public string Code { get; }
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Create(string code, string message) => new(code, message);

    public static implicit operator string(Error error) => error.Code;
}
