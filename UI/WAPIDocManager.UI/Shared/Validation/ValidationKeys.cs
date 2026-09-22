using WAPIDocManager.UI.Shared.Forms;
using WAPIDocManager.UI;

namespace WAPIDocManager.UI.Shared.Validation;

/// <summary>
/// Chiavi dei messaggi di validazione: il testo localizzato è risolto dalla UI tramite IStringLocalizer
/// </summary>
/// <remarks>
/// <para>
/// Le costanti vanno usate come <c>ErrorMessage</c> degli attributi DataAnnotations dei form model: Application resta
/// indipendente dalla localizzazione e <c>Shared/Forms/FieldValidationMessage</c> traduce la chiave.
/// </para>
/// <para>
/// Ogni nuova chiave va aggiunta in ENTRAMBI i file <c>WAPIDocManager.UI/Resources/SharedResource.resx</c> (italiano)
/// e <c>SharedResource.en.resx</c> (inglese); una chiave mancante viene mostrata così com'è.
/// Le chiavi non devono contenere segnaposto <c>{0}</c> (ValidationAttribute.FormatErrorMessage li sostituirebbe con il nome del campo).
/// </para>
/// </remarks>
public static class ValidationKeys
{
    /// <summary>Fallback per errori senza messaggio.</summary>
    public const string Invalid = "Validation_Invalid";
    public const string Required = "Validation_Required";
    public const string EmailInvalid = "Validation_EmailInvalid";
    public const string GreaterThanZero = "Validation_GreaterThanZero";

    /// <summary>Quantità riga: precisione 9, scala 2.</summary>
    public const string QuantityPrecision = "Validation_QuantityPrecision";

    /// <summary>Prezzo unitario riga: precisione 12, scala 2.</summary>
    public const string UnitPricePrecision = "Validation_UnitPricePrecision";

    public const string PageSizeRange = "Validation_PageSizeRange";
}
