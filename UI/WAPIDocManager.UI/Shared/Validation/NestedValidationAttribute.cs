using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Shared.Validation;

/// <summary>
/// Indica a <see cref="ModelValidator"/> di validare anche l'oggetto (o gli elementi della collezione) della proprietà
/// </summary>
/// <remarks>
/// Serve perché il <c>DataAnnotationsValidator</c> standard di Blazor valida solo le proprietà di primo livello
/// del modello e non entra negli oggetti annidati né nelle collezioni (es. <c>DocumentEditModel.Lines</c>).
/// Senza questo attributo le proprietà annidate NON vengono validate.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NestedValidationAttribute : Attribute
{
}
