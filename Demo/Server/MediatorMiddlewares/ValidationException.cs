namespace Demo.Server.MediatorMiddlewares;

/// <summary>
/// Raised by <see cref="ValidatorMiddleware"/> when an action implementing <see cref="Demo.Shared.IValidable"/> reports
/// validation errors. Translated back into client-facing messages by <see cref="ValidationExceptionHandler"/>.
/// </summary>
public class ValidationException(string[] errors) : Exception("Validation failed.")
{
    public string[] Errors { get; } = errors;
}
