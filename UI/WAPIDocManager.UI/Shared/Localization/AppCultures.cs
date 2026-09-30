using System.Globalization;
using Microsoft.JSInterop;

namespace WAPIDocManager.UI.Shared.Localization;

/// Lingua selezionabile nell'interfaccia: il nome della cultura, che viene anche salvato fra le preferenze del
/// browser, e il nome mostrato nel selettore, scritto sempre nella lingua stessa.
public sealed record AppCulture(string Name, string DisplayName);

/// Lingue supportate dall'interfaccia; l'italiano è quella predefinita.
/// In WebAssembly la cultura va impostata prima che l'applicazione parta, perché è in quel momento che vengono
/// caricate le risorse tradotte della lingua corrente. È la ragione per cui il cambio di lingua salva la scelta e
/// ricarica la pagina, invece di limitarsi a ridisegnare i componenti.
/// Per aggiungere una lingua servono tre cose: una voce qui, il file di risorse con tutte le chiavi tradotte, e la
/// lingua fra quelle incluse nel bundle, dichiarate nel file di progetto.
public static class AppCultures
{
    /// Usata alla prima visita, o quando la lingua salvata non è più fra quelle supportate.
    public const string Default = "it-IT";

    public static IReadOnlyList<AppCulture> Supported { get; } = new[]
    {
        new AppCulture("it-IT", "Italiano"),
        new AppCulture("en-US", "English")
    };

    /// Applica la lingua salvata fra le preferenze del browser ai formati e ai testi dell'interfaccia.
    public static async Task ApplyStoredCultureAsync(IJSRuntime jsRuntime)
    {
        string? storedCulture = await jsRuntime.InvokeAsync<string?>("wapiPreferences.getCulture");

        string cultureName = Supported.Any(culture => culture.Name == storedCulture)
            ? storedCulture!
            : Default;

        // la cultura governa i formati di date e numeri, quella dell'interfaccia la scelta del file di risorse
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // aggiorna la lingua dichiarata dal documento: serve all'accessibilità e al controllo ortografico del browser
        await jsRuntime.InvokeVoidAsync("wapiPreferences.setDocumentLanguage", culture.TwoLetterISOLanguageName);
    }
}
