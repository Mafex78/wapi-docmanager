using WAPIDocManager.UI.Shared.Forms;
using WAPIDocManager.UI;

namespace WAPIDocManager.UI.Shared.Validation;

/// <summary>
/// Chiavi dei messaggi di validazione: il testo localizzato è risolto dalla UI tramite IStringLocalizer
/// </summary>
/// <remarks>
/// <para>
/// Le costanti si usano con <c>WithMessage(...)</c> nei validator FluentValidation degli slice
/// (<c>Features/&lt;Area&gt;/Validators</c>): le regole restano indipendenti dalla localizzazione e
/// <c>Shared/Forms/FieldValidationMessage</c> traduce la chiave al momento della resa.
/// </para>
/// <para>
/// Ogni nuova chiave va aggiunta in ENTRAMBI i file <c>WAPIDocManager.UI/Resources/SharedResource.resx</c> (italiano)
/// e <c>SharedResource.en.resx</c> (inglese); una chiave mancante viene mostrata così com'è.
/// Le chiavi non devono contenere segnaposto di FluentValidation (<c>{PropertyName}</c>, <c>{ComparisonValue}</c>):
/// il testo tradotto arriva dal .resx, non dal messaggio della regola.
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
