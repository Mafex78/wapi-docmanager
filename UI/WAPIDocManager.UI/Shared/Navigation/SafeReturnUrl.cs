using System.Diagnostics.CodeAnalysis;

namespace WAPIDocManager.UI.Shared.Navigation;

/// <summary>
/// Verifica se un indirizzo di ritorno letto dalla query string è utilizzabile per navigare
/// </summary>
/// <remarks>
/// <para>
/// VALIDA, non sanifica: il valore non viene mai modificato, viene accettato o rifiutato.
/// </para>
/// <para>
/// PERCHÉ ESISTE: la query string la scrive chiunque, quindi un link confezionato ad arte potrebbe portare l'utente
/// fuori dall'applicazione (open redirect). Sono accettati SOLO percorsi relativi a <c>&lt;base href&gt;</c>:
/// niente slash o backslash iniziale (aprirebbero a <c>//host</c> e <c>\\host</c>, che il browser tratta come
/// indirizzi assoluti), niente URL assoluti, e niente ritorno alle pagine vietate indicate dal chiamante
/// (es. il login stesso, per non creare un ciclo).
/// </para>
/// <para>
/// Chi decide cosa fare del rifiuto è <see cref="ReturnUrlResolver"/>: qui non c'è alcun ripiego, perché scegliere
/// la destinazione alternativa non è compito di chi valida.
/// </para>
/// </remarks>
public static class SafeReturnUrl
{
    /// <summary>
    /// Restituisce true e valorizza <paramref name="safeUrl"/> se il candidato è un percorso relativo accettabile.
    /// </summary>
    /// <param name="candidate">Valore arrivato dalla query string (può essere null).</param>
    /// <param name="safeUrl">Il candidato stesso quando è accettabile, altrimenti null.</param>
    /// <param name="forbiddenPrefixes">Percorsi da rifiutare anche se relativi, confrontati senza distinzione di maiuscole.</param>
    public static bool TryGetSafeRelativeUrl(
        string? candidate,
        [NotNullWhen(true)] out string? safeUrl,
        params string[] forbiddenPrefixes)
    {
        safeUrl = null;

        if (string.IsNullOrWhiteSpace(candidate) ||
            candidate.StartsWith('/') ||
            candidate.StartsWith('\\') ||
            !Uri.IsWellFormedUriString(candidate, UriKind.Relative))
        {
            return false;
        }

        bool isForbidden = forbiddenPrefixes
            .Any(prefix => candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (isForbidden)
        {
            return false;
        }

        safeUrl = candidate;
        return true;
    }
}
