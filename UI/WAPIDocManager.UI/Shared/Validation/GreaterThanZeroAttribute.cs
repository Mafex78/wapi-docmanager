using System.ComponentModel.DataAnnotations;

namespace WAPIDocManager.UI.Shared.Validation;

/// <summary>
/// Valore numerico strettamente maggiore di zero (equivalente a <c>GreaterThan(0)</c> di FluentValidation).
/// </summary>
/// <remarks>
/// Attributo custom invece di <c>[Range]</c>: Range con decimal interpreta i limiti stringa secondo la cultura corrente.
/// null è considerato valido (l'obbligatorietà si esprime con <c>[Required]</c>).
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GreaterThanZeroAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value switch
        {
            null => true,
            decimal number => number > 0,
            int number => number > 0,
            _ => false
        };
    }
}
