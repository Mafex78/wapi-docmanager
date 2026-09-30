using System.Diagnostics.CodeAnalysis;

namespace WAPIDocManager.UI.Shared.Navigation;

/// Verifica se un indirizzo di ritorno letto dalla query string è utilizzabile per navigare.
/// VALIDA, non sanifica: il valore non viene mai modificato, viene accettato o rifiutato.
/// Serve perché la query string la scrive chiunque, quindi un collegamento confezionato ad arte potrebbe portare
/// l'utente fuori dall'applicazione. Si accettano SOLO percorsi relativi all'indirizzo base: niente slash o
/// backslash iniziale, che il browser interpreterebbe come indirizzi assoluti verso un altro host, niente indirizzi
/// assoluti, e niente ritorno alle pagine che il chiamante dichiara vietate — la pagina di accesso rifiuta se
/// stessa, per non creare un ciclo.
/// Cosa fare di un valore rifiutato non si decide qui: scegliere la destinazione alternativa non è compito di chi
/// valida.
public static class SafeReturnUrl
{
    /// Restituisce true, valorizzando l'indirizzo, se il candidato è un percorso relativo accettabile.
    /// I prefissi vietati vengono confrontati senza distinzione fra maiuscole e minuscole.
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
