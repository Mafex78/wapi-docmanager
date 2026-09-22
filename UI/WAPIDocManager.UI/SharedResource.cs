namespace WAPIDocManager.UI;

/// <summary>
/// Marker delle risorse localizzate condivise (Resources/SharedResource.resx).
/// Resta nella root del progetto perché il nome della risorsa sia WAPIDocManager.UI.Resources.SharedResource.
/// </summary>
/// <remarks>
/// <para>
/// Il localizzatore costruisce il nome della risorsa come namespace radice + ResourcesPath ("Resources", vedi Program.cs)
/// + nome del tipo. Spostare questa classe nella cartella Resources (accanto al .resx) cambierebbe il nome della risorsa
/// incorporata (convenzione DependentUpon) e tutti i testi verrebbero mostrati come chiavi.
/// </para>
/// <para>
/// Uso: <c>@inject IStringLocalizer&lt;SharedResource&gt; L</c> (già in _Imports.razor), poi <c>@L["Chiave"]</c>
/// o <c>L["Chiave", valore]</c> per i testi con segnaposto {0}. Convenzioni delle chiavi: commento in testa a SharedResource.resx.
/// </para>
/// </remarks>
public sealed class SharedResource
{
}
