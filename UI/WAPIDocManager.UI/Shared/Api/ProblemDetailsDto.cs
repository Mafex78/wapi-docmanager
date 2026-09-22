namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Corpo delle risposte di errore (<c>application/problem+json</c>).
/// </summary>
/// <remarks>
/// Due origini lato server: <c>Shared.Application/GlobalExceptionHandler.cs</c> (Title sempre "Error", messaggio in <see cref="Detail"/>)
/// e la validazione automatica di [ApiController] sul model binding (ValidationProblemDetails, messaggi in <see cref="Errors"/>).
/// Letto da <c>Http/ApiResponseReader</c>.
/// </remarks>
public record ProblemDetailsDto
{
    public string? Type { get; init; }
    public string? Title { get; init; }
    public int? Status { get; init; }
    public string? Detail { get; init; }

    /// <summary>
    /// Presente nei ValidationProblemDetails (model binding / validazione automatica)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
}
