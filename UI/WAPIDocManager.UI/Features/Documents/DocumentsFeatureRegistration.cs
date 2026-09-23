using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Validators;
using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Features.Documents.Views.Pages;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents;

/// <summary>
/// Slice "Documenti": ricerca, creazione, modifica, stati, generazione, collegamenti ed eliminazione
/// </summary>
/// <remarks>
/// <para>
/// CONTENUTO DELLO SLICE (una cartella per ruolo, non per caso d'uso)
/// <list type="bullet">
///   <item><c>Views/Pages/</c> – DocumentList, DocumentCreate, DocumentDetail, DocumentEdit, con le partial
///         .razor.cs delle pagine più lunghe.</item>
///   <item><c>Views/Components/</c> – DocumentForm (condiviso da creazione e modifica), DocumentLinesEditor, StatusBadge.</item>
///   <item><c>ViewModels/</c> – logica delle pagine che ne hanno (MVVM solo dove serve: DocumentCreate non ne ha),
///         più DocumentDetailNotification, l'esito che il ViewModel restituisce alla pagina. Testati senza Blazor.</item>
///   <item><c>Entities/</c> – ciò che torna dalle API già trasformato per l'app: Document, Customer, DocumentLine,
///         DocumentLink, PagedResult, gli enum e DocumentRules (regole di stato SPECULARI a quelle del server).</item>
///   <item><c>Models/</c> – modelli di form e di filtro con la validazione: DocumentEditModel, CustomerEditModel,
///         DocumentLineEditModel, DocumentFilter, DocumentSortFields, SortDirection.</item>
///   <item><c>Validators/</c> – regole dei form (FluentValidation): documento, cliente annidato, righe e filtri.</item>
///   <item><c>Services/</c> – IDocumentService + DocumentApiService (tutti gli endpoint di WAPIDocument),
///         DocumentQueryStringBuilder e DocumentMapper.</item>
///   <item><c>Contracts/</c> – forme di trasporto (DocumentResponse, DocumentCreateRequest, PageDto, ...) con gli
///         operatori di conversione espliciti.</item>
///   <item>in radice: DocumentPermissions e PermissionDenialReason (ruolo × stato), DocumentListState (filtri che
///         sopravvivono alla navigazione), DocumentLabels (etichette localizzate degli enum).</item>
/// </list>
/// </para>
/// <para>
/// PERCHÉ UNA SOLA FUNZIONALITÀ E NON UNA PER CASO D'USO (CreateDocument, EditDocument, ...): creazione e modifica
/// condividono DocumentForm, DocumentEditModel, le entità, i permessi e lo stesso typed HttpClient — circa l'80% del
/// materiale. Due slice separati produrrebbero subito una terza cartella comune, cioè questa con un nome diverso.
/// Se la funzionalità crescerà, la divisione naturale è per sotto-area (es. Documents/Collegamenti), non per verbo.
/// </para>
/// <para>
/// PagedResult e PageDto stanno qui e non in Shared/Api perché oggi li usa solo questo slice: si promuovono a Shared
/// quando una seconda funzionalità pagina i risultati (spostamento di due file).
/// </para>
/// </remarks>
public static class DocumentsFeatureRegistration
{
    public static IServiceCollection AddDocumentsFeature(this IServiceCollection services, IConfiguration configuration)
    {
        ApiOptions apiOptions = configuration.ReadApiOptions();

        services.AddHttpClient<IDocumentService, DocumentApiService>(client =>
                client.BaseAddress = apiOptions.DocumentBaseAddress())
            .AddHttpMessageHandler<BearerTokenHandler>();

        // regole dei form: risolte dalla DI dal componente <FluentValidator /> (Blazilla); senza stato, quindi Singleton.
        // Quello del documento delega ai validator del cliente e delle righe (SetValidator / RuleForEach).
        services.AddSingleton<IValidator<DocumentEditModel>, DocumentEditModelValidator>();
        services.AddSingleton<IValidator<DocumentFilter>, DocumentFilterValidator>();

        // in WebAssembly lo scope dura quanto la scheda: i filtri della lista sopravvivono alla navigazione
        services.AddScoped<DocumentListState>();

        // ViewModel delle pagine con logica (MVVM): Transient, così ogni visita parte da uno stato pulito
        // (con Scoped lo stato durerebbe quanto la scheda e si ritroverebbe la visita precedente)
        services.AddTransient<DocumentListViewModel>();
        services.AddTransient<DocumentDetailViewModel>();
        services.AddTransient<DocumentEditViewModel>();

        return services;
    }
}
