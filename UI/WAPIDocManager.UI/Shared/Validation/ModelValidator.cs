using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using WAPIDocManager.UI.Shared.Forms;

namespace WAPIDocManager.UI.Shared.Validation;

/// <remarks>
/// REGOLA DI Shared/: qui sta solo ciò che DUE O PIÙ funzionalità usano davvero — la validazione serve ai form di
/// documenti, login e registrazione utenti. Un tipo usato da un solo slice resta nello slice e si promuove qui quando
/// compare il secondo utilizzatore; vale anche il contrario (riportare nello slice ciò che è rimasto usato da uno solo).
/// </remarks>
/// <summary>
/// Validazione DataAnnotations dell'intero grafo del modello
/// (le proprietà marcate con <see cref="NestedValidationAttribute"/> vengono validate ricorsivamente)
/// </summary>
/// <remarks>
/// <para>
/// Usata dai form tramite <c>WAPIDocManager.UI/Shared/Forms/ModelGraphValidator</c> (al posto di DataAnnotationsValidator)
/// e testabile senza Blazor (<c>Tests/.../Validation/ModelValidatorTests.cs</c>).
/// </para>
/// <para>
/// Usa la reflection sulle proprietà dei modelli: per questo il progetto UI dichiara
/// <c>TrimmerRootAssembly</c> per questo assembly (il trimming in publish rimuoverebbe le proprietà "inutilizzate").
/// </para>
/// </remarks>
public static class ModelValidator
{
    /// <summary>
    /// Valida <paramref name="model"/> e tutti gli oggetti annidati marcati con <see cref="NestedValidationAttribute"/>.
    /// </summary>
    /// <returns>Elenco degli errori (vuoto se il modello è valido).</returns>
    public static IReadOnlyList<ValidationFailure> Validate(object model)
    {
        var failures = new List<ValidationFailure>();

        // confronto per riferimento: evita cicli e doppie validazioni della stessa istanza
        ValidateRecursive(model, failures, new HashSet<object>(ReferenceEqualityComparer.Instance));
        return failures;
    }

    private static void ValidateRecursive(
        object model,
        List<ValidationFailure> failures,
        HashSet<object> visited)
    {
        if (!visited.Add(model))
        {
            return;
        }

        // validateAllProperties: true = valuta tutti gli attributi, non solo [Required]
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        foreach (ValidationResult result in results)
        {
            // ErrorMessage contiene la chiave di ValidationKeys impostata sull'attributo
            string messageKey = result.ErrorMessage ?? ValidationKeys.Invalid;

            foreach (string memberName in result.MemberNames.DefaultIfEmpty(string.Empty))
            {
                failures.Add(new ValidationFailure(model, memberName, messageKey));
            }
        }

        foreach (PropertyInfo property in model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<NestedValidationAttribute>() is null)
            {
                continue;
            }

            object? value = property.GetValue(model);

            // string è IEnumerable (di char): va esclusa esplicitamente
            if (value is null or string)
            {
                continue;
            }

            if (value is IEnumerable items)
            {
                foreach (object? item in items)
                {
                    if (item is not null)
                    {
                        ValidateRecursive(item, failures, visited);
                    }
                }
            }
            else
            {
                ValidateRecursive(value, failures, visited);
            }
        }
    }
}
