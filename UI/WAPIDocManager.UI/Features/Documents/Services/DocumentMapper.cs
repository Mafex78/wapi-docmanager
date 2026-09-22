using WAPIDocManager.UI.Features.Documents.Contracts;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// <summary>
/// Utility condivise dagli operatori espliciti dei contratti.
/// </summary>
/// <remarks>
/// Le conversioni tra contratti wire e modelli del client sono definite come <c>explicit operator</c> nei singoli DTO
/// di <c>Contracts/</c> (stesso schema dei DTO del backend) e si usano con un cast, es. <c>(DocumentCreateRequest)model</c>.
/// Qui restano solo le funzioni non associabili a un'entità.
/// </remarks>
public static class DocumentMapper
{
    /// <summary>
    /// Stringhe vuote o di soli spazi → null; altrimenti trim.
    /// </summary>
    /// <remarks>Usata per i campi testuali inviati alle API (dati cliente, descrizione riga).</remarks>
    public static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
