using System.Text.Json;

namespace WAPIDocManager.UI.Shared.Api;

/// Le stesse impostazioni JSON usate dal server: nomi in camelCase, lettura senza distinzione di maiuscole,
/// enum come numeri.
/// Valgono per tutte le chiamate HTTP e per la sessione salvata nel browser. Gli enum restano numeri: convertirli
/// in stringhe qui, o là, rompe la compatibilità fra i due lati.
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
