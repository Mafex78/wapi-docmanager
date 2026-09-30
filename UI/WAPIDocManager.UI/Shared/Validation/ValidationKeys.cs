namespace WAPIDocManager.UI.Shared.Validation;

/// Chiavi dei messaggi di validazione: le regole dei form indicano una chiave, non un testo, e la traduzione
/// avviene nel momento in cui il messaggio viene mostrato. Così le regole restano indipendenti dalla lingua.
/// Ogni chiave nuova va aggiunta in ENTRAMBI i file di risorse, italiano e inglese; una chiave mancante viene
/// mostrata all'utente così com'è.
/// Le chiavi non devono contenere i segnaposto della libreria di validazione, come il nome della proprietà o il
/// valore di confronto: il testo tradotto arriva dalle risorse, non dal messaggio della regola.
public static class ValidationKeys
{
    /// Ripiego per gli errori senza messaggio.
    public const string Invalid = "Validation_Invalid";
    public const string Required = "Validation_Required";
    public const string EmailInvalid = "Validation_EmailInvalid";
    public const string GreaterThanZero = "Validation_GreaterThanZero";

    /// Quantità di riga: precisione 9, scala 2.
    public const string QuantityPrecision = "Validation_QuantityPrecision";

    /// Prezzo unitario di riga: precisione 12, scala 2.
    public const string UnitPricePrecision = "Validation_UnitPricePrecision";

    public const string PageSizeRange = "Validation_PageSizeRange";
}
