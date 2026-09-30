using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Validators;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents;

/// Slice "Documenti": ricerca, creazione, modifica, cambio di stato, generazione, collegamenti ed eliminazione.
/// Mappa dello slice — una cartella per ruolo, non per caso d'uso:
///   Views/Pages/        elenco, creazione, dettaglio e modifica, con il codice separato per le pagine più lunghe;
///   Views/Components/   il form condiviso da creazione e modifica, l'editor delle righe, il contrassegno di stato;
///   ViewModels/         la logica delle pagine che ne hanno, più l'esito che il ViewModel restituisce alla pagina.
///                       Sono verificabili senza renderizzare nulla;
///   Entities/           ciò che torna dalle API, già trasformato per l'applicazione, gli enum, e le regole di stato,
///                       che sono SPECULARI a quelle applicate dal server;
///   Models/             modelli di form e di filtro;
///   Validators/         le regole dei form: documento, cliente annidato, righe e filtri;
///   Services/           le chiamate alle API, la costruzione della query string e la mappatura;
///   Services/Dto/       le forme di trasporto, con gli operatori di conversione espliciti;
///   in radice           permessi e motivo del rifiuto (ruolo per stato), i filtri che sopravvivono alla
///                       navigazione, le etichette tradotte degli enum.
/// PERCHÉ UNA SOLA FUNZIONALITÀ E NON UNA PER CASO D'USO: creazione e modifica condividono form, modello, entità,
/// permessi e lo stesso client HTTP, cioè circa l'ottanta per cento del materiale. Due funzionalità separate
/// produrrebbero subito una terza cartella comune, che sarebbe questa con un altro nome. Se crescerà, la divisione
/// naturale è per sotto-area, non per verbo.
/// La pagina di risultati e la sua forma di trasporto stanno qui e non fra i componenti condivisi perché oggi le usa
/// solo questa funzionalità: si promuovono quando una seconda funzionalità pagina i propri risultati.
public static class DocumentsFeatureRegistration
{
    public static IServiceCollection AddDocumentsFeature(this IServiceCollection services, IConfiguration configuration)
    {
        ApiOptions apiOptions = configuration.ReadApiOptions();

        services.AddHttpClient<IDocumentService, DocumentApiService>(client =>
                client.BaseAddress = apiOptions.DocumentBaseAddress())
            .AddHttpMessageHandler<BearerTokenHandler>();

        // regole dei form: risolte dalle dipendenze dal componente di validazione; senza stato, quindi Singleton.
        // Quella del documento delega alle regole del cliente e delle righe.
        services.AddSingleton<IValidator<DocumentEditModel>, DocumentEditModelValidator>();
        services.AddSingleton<IValidator<DocumentFilter>, DocumentFilterValidator>();

        // in WebAssembly lo scope dura quanto la scheda del browser: i filtri dell'elenco sopravvivono alla navigazione
        services.AddScoped<DocumentListState>();

        // Transient, così ogni visita a una pagina parte da uno stato pulito: con Scoped lo stato durerebbe quanto
        // la scheda e si ritroverebbe quello della visita precedente
        services.AddTransient<DocumentListViewModel>();
        services.AddTransient<DocumentDetailViewModel>();
        services.AddTransient<DocumentEditViewModel>();

        return services;
    }
}
