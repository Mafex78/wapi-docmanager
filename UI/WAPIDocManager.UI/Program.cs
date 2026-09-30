using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using WAPIDocManager.UI;
using WAPIDocManager.UI.Features.Auth;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Features.Users;
using Blazing.Mvvm;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Localization;
using WAPIDocManager.UI.Shared.Navigation;

// Punto di composizione del client.
// Struttura del progetto, a vertical slice: Features/<Area>/ contiene TUTTO ciò che serve a una funzionalità —
// pagine, componenti, entità, modelli di form, chiamate alle API e forme di trasporto — mentre Shared/ contiene
// solo ciò che due o più funzionalità usano davvero. Ogni funzionalità espone la propria registrazione, e il
// commento in testa a quella classe è il punto da cui partire per capirla.

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// il componente radice viene montato nel segnaposto della pagina iniziale; il secondo abilita il titolo dinamico
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1) supporto MVVM, che serve alla classe base dei componenti con ViewModel.
//    I ViewModel restano registrati esplicitamente dalle funzionalità: la registrazione automatica non sovrascrive
//    quelle già presenti, quindi le nostre hanno comunque la precedenza.
builder.Services.AddMvvm(options => options.HostingModelType = BlazorHostingModelType.WebAssembly);

// 2) infrastruttura HTTP condivisa: PRIMA delle funzionalità, che ne usano il gestore del token e gli indirizzi base
builder.Services.AddApiInfrastructure(builder.Configuration);

// 3) una registrazione per funzionalità: client HTTP, servizi, stato e ViewModel
builder.Services.AddAuthFeature(builder.Configuration);
builder.Services.AddDocumentsFeature(builder.Configuration);
builder.Services.AddUsersFeature(builder.Configuration);

// 4) navigazione: validazione degli indirizzi di ritorno presi dalla query string
builder.Services.AddScoped<ReturnUrlResolver>();

// 5) autenticazione: lo stato viene reso disponibile a tutti i componenti come parametro a cascata, e viene
//    ricavato dalla sessione salvata nel browser
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// 6) traduzioni
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

WebAssemblyHost host = builder.Build();

// la lingua va impostata prima dell'avvio, perché è all'avvio che vengono caricate le risorse tradotte:
// per questo il cambio di lingua ricarica la pagina
await AppCultures.ApplyStoredCultureAsync(host.Services.GetRequiredService<IJSRuntime>());

await host.RunAsync();
