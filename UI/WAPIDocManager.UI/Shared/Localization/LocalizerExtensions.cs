using Microsoft.Extensions.Localization;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Localization;

/// Etichette localizzate dei tipi usati da più funzionalità: oggi solo i ruoli, che servono alla registrazione
/// utenti, ai permessi sui documenti e alla barra superiore.
/// Le etichette che servono a una sola funzionalità restano dentro quella funzionalità: è la regola generale dei
/// componenti condivisi.
/// Le chiavi sono costruite dal nome del valore e vanno aggiunte in entrambi i file di risorse.
public static class LocalizerExtensions
{
    public static string Label(this IStringLocalizer localizer, RoleType role)
    {
        return localizer[$"RoleType_{role}"];
    }
}
