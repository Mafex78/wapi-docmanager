using Microsoft.Extensions.Localization;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Localization;

/// <summary>
/// Etichette localizzate dei tipi trasversali a più funzionalità
/// </summary>
/// <remarks>
/// Qui resta solo ciò che serve a due o più slice: oggi <see cref="RoleType"/>, usato dalla registrazione utenti,
/// dai permessi sui documenti e dalla barra superiore. Le etichette specifiche di una funzionalità stanno nello slice
/// (es. <c>Features/Documents/DocumentLabels.cs</c>): è la regola generale di <c>Shared/</c>.
/// Le chiavi sono costruite dal nome del valore (es. <c>RoleType_Admin</c>) e vanno aggiunte in entrambi i .resx.
/// </remarks>
public static class LocalizerExtensions
{
    /// <summary>Chiave <c>RoleType_{Valore}</c>.</summary>
    public static string Label(this IStringLocalizer localizer, RoleType role)
    {
        return localizer[$"RoleType_{role}"];
    }
}
