using System.Globalization;
using Microsoft.JSInterop;
using WAPIDocManager.UI;

namespace WAPIDocManager.UI.Shared.Localization;

/// <summary>
/// Lingua selezionabile nell'interfaccia.
/// </summary>
/// <param name="Name">Nome della cultura .NET (es. it-IT), salvato nel localStorage.</param>
/// <param name="DisplayName">Nome mostrato nel selettore, sempre nella lingua stessa.</param>
public sealed record AppCulture(string Name, string DisplayName);

/// <summary>
/// Culture supportate dall'interfaccia (italiano predefinito)
/// </summary>
/// <remarks>
/// <para>
/// In Blazor WebAssembly la cultura si imposta prima di <c>host.RunAsync()</c> (Program.cs), che carica le satellite assembly
/// dei .resx per la cultura corrente. Per questo il cambio lingua (Shared/Components/CultureSelector) salva la scelta e ricarica la pagina.
/// </para>
/// <para>
/// Per aggiungere una lingua: una voce in <see cref="Supported"/>, il file <c>Resources/SharedResource.{lingua}.resx</c>
/// con tutte le chiavi e la lingua in <c>SatelliteResourceLanguages</c> del csproj.
/// </para>
/// </remarks>
public static class AppCultures
{
    /// <summary>Usata alla prima visita o se la cultura salvata non è più supportata.</summary>
    public const string Default = "it-IT";

    public static IReadOnlyList<AppCulture> Supported { get; } = new[]
    {
        new AppCulture("it-IT", "Italiano"),
        new AppCulture("en-US", "English")
    };

    /// <summary>
    /// Applica la cultura salvata (window.wapiPreferences in wwwroot/js/app.js) a formati e testi dell'interfaccia.
    /// </summary>
    public static async Task ApplyStoredCultureAsync(IJSRuntime jsRuntime)
    {
        string? storedCulture = await jsRuntime.InvokeAsync<string?>("wapiPreferences.getCulture");

        string cultureName = Supported.Any(culture => culture.Name == storedCulture)
            ? storedCulture!
            : Default;

        // CurrentCulture: formati di date e numeri; CurrentUICulture: scelta del file .resx
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // aggiorna <html lang> (accessibilità, controllo ortografico del browser)
        await jsRuntime.InvokeVoidAsync("wapiPreferences.setDocumentLanguage", culture.TwoLetterISOLanguageName);
    }
}
