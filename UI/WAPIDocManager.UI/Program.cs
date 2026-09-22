using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using WAPIDocManager.UI;
using WAPIDocManager.UI.Features.Auth;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Features.Users;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Localization;

// Composition root del client Blazor WebAssembly.
//
// Struttura del progetto (vertical slice): Features/<Area>/ contiene TUTTO ciò che serve a una funzionalità
// (pagine, componenti, entità, modelli di form, servizi API, contratti wire); Shared/ contiene solo ciò che due o più
// funzionalità usano davvero. Ogni slice espone la propria registrazione: il commento in testa a quella classe
// (es. Features/Documents/DocumentsFeatureRegistration.cs) descrive la funzionalità ed è il punto da cui partire.

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// App viene montato nel <div id="app"> di wwwroot/index.html; HeadOutlet abilita <PageTitle>
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1) infrastruttura HTTP condivisa: PRIMA degli slice, che ne usano BearerTokenHandler e gli URL base
//    (sezione "Api" di wwwroot/appsettings.json, vedi Shared/Api/ApiOptions.cs)
builder.Services.AddApiInfrastructure(builder.Configuration);

// 2) una registrazione per funzionalità: typed HttpClient, servizi, stato e ViewModel dello slice
builder.Services.AddAuthFeature(builder.Configuration);
builder.Services.AddDocumentsFeature(builder.Configuration);
builder.Services.AddUsersFeature(builder.Configuration);

// 3) autenticazione JWT
// - AddCascadingAuthenticationState: Task<AuthenticationState> disponibile a tutti i componenti come [CascadingParameter]
// - JwtAuthenticationStateProvider: ricava l'utente da IUserSessionStore (registrato in AddApiInfrastructure)
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// 4) localizzazione (Resources/SharedResource*.resx)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

WebAssemblyHost host = builder.Build();

// la cultura va impostata prima dell'avvio: al cambio lingua la pagina viene ricaricata
// (RunAsync carica le satellite assembly della cultura corrente)
await AppCultures.ApplyStoredCultureAsync(host.Services.GetRequiredService<IJSRuntime>());

await host.RunAsync();
