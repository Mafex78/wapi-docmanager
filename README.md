# WAPIDocManager

Sistema di gestione documenti commerciali (preventivi, proforme e ordini di vendita) realizzato in **.NET 8** secondo un'architettura a microservizi, con persistenza su **MongoDB** e autenticazione basata su **JWT**.

La solution è composta da due microservizi indipendenti (`WAPIIdentity` e `WAPIDocument`) che condividono tre librerie trasversali (`Shared.Domain`, `Shared.Application`, `Shared.Infrastructure`), oltre a quattro progetti di test automatizzati. Il formato della solution è il nuovo `.slnx` (XML solution format).

Nella cartella `UI/` è presente una solution separata (`WAPIDocManager.UI.slnx`) con il client web **Blazor WebAssembly Standalone** che si autentica su `WAPIIdentity` e offre l'interfaccia grafica per tutti gli endpoint di `WAPIDocument` (vedi [Client UI](#client-ui-blazor-webassembly)).

---

## Parte 1 – Analisi e architettura

### 1. Architettura a microservizi

La soluzione è suddivisa in due microservizi principali, pensati per essere eseguiti, scalati e versionati in modo indipendente. Ogni microservizio possiede il proprio database MongoDB dedicato ed è strutturato secondo i principi della Clean Architecture (Domain / Application / Infrastructure / API).

```
WAPIDocManager.slnx
├── WAPIIdentity (microservizio)
│   ├── WAPIIdentity              (API)
│   ├── WAPIIdentity.Application
│   ├── WAPIIdentity.Domain
│   └── WAPIIdentity.Infrastructure
├── WAPIDocument (microservizio)
│   ├── WAPIDocument              (API)
│   ├── WAPIDocument.Application
│   ├── WAPIDocument.Domain
│   └── WAPIDocument.Infrastructure
├── Shared (librerie trasversali)
│   ├── Shared.Application
│   ├── Shared.Domain
│   └── Shared.Infrastructure
└── Tests
    ├── WAPIIdentity.Api.Tests
    ├── WAPIIdentity.Application.Tests
    ├── WAPIDocument.Api.Tests
    └── WAPIDocument.Application.Tests
```

#### Identity Service (`WAPIIdentity`)

- **Responsabilità**
  - Autenticazione degli utenti tramite credenziali (email + password).
  - Emissione di **token JWT** firmati con algoritmo HMAC-SHA256.
  - Gestione utenti (registrazione, ruoli).
  - Gestione dei ruoli applicativi tramite l'enum `RoleType` (`Admin`, `Editor`, `Viewer`).
- **Confini funzionali**
  - Il servizio è la fonte ufficiale e unica sull'identità: nessun altro microservizio può creare, aggiornare o validare utenti.
  - Non conosce peculiarità del dominio documentale.
- **Modalità di comunicazione**
  - **REST** sincrono, esposto tramite ASP.NET Core Web API (endpoint `AuthController`, `UsersController`).
  - Nessuna comunicazione esterna verso altri microservizi.
- **Sicurezza**
  - Endpoint di autenticazione `POST /api/v1/auth/login` pubblico (`[AllowAnonymous]`).
  - Endpoint di registrazione utente protetto e riservato al ruolo `Admin`.
  - Opzioni JWT (`Issuer`, `Audience`, `SigningKey`, `ExpirationMinutes`) configurate via `JwtOptions` e caricate da `appsettings.json`.
  - Validazione delle richieste tramite **FluentValidation**.

#### Document Service (`WAPIDocument`)

- **Responsabilità**
  - Gestione del ciclo di vita dei documenti commerciali: **Quote** (preventivo), **Proforma** e **SalesOrder** (ordine di vendita).
  - Gestione degli stati del documento e delle relative regole di avanzamento.
  - Tracciamento delle relazioni/collegamenti tra documenti (es. Preventivo → Proforma → Ordine) tramite entità `DocumentLink`.
  - Gestione delle entità fiscali embedded (`Customer`, `DocumentLine`).
- **Confini funzionali**
  - Il servizio non gestisce utenti o autenticazione: si fida del token JWT emesso da `WAPIIdentity`, di cui verifica firma, issuer, audience e scadenza.
  - Espone CRUD e operazioni di dominio (generazione documento da documento esistente, cambio di stato, ricerca paginata e filtrata, collegamento manuale tra documenti).
- **Modalità di comunicazione**
  - **REST** sincrono (endpoint `DocumentsController`, route `api/v{version:apiVersion}/documents`).
  - Nessuna comunicazione esterna verso altri microservizi.
- **Sicurezza**
  - Tutti gli endpoint sono protetti da `[Authorize]` con controllo di ruolo:
    - `Viewer` e `Admin` possono leggere (`GET`).
    - `Editor` e `Admin` possono creare, aggiornare, cambiare stato, generare ed eliminare.
  - Validazione input tramite **FluentValidation**.
  - Verifica del JWT.

### 2. Backend .NET

#### Versione di .NET utilizzata

Tutti i progetti della solution hanno come target framework **.NET 8.0** (`net8.0`).

#### Pattern architetturali

La solution adotta un mix di pattern consolidati:

- **Clean Architecture** (Onion Architecture) su ciascun microservizio: la dipendenza punta sempre dall'esterno verso il Domain. Il progetto `Domain` non ha dipendenze da Infrastructure o Api.
- **Domain-Driven Design (DDD) tattico**: entità con identità (`IEntity<T>`), value object immutabili (`Customer`, `DocumentLine`), aggregate root (`Document`) che incapsula regole di invariante (validazione, transizioni di stato, generazione e collegamento di documenti).
- **Repository Pattern** + **Unit of Work**: `IDocumentRepository`, `IUserRepository`, `IUnitOfWork` astraggono l'accesso al database; le implementazioni vivono nei rispettivi layer `*.Infrastructure`.
- **Service Layer**: la logica applicativa è orchestrata da servizi (`DocumentService`, `UserService`, `LoginService`) che ricevono DTO, invocano il dominio e persistono tramite repository.
- **DTO + Validation**: ogni operazione API accetta un Request DTO validato tramite FluentValidation e restituisce un Response DTO. Il dominio non viene mai esposto direttamente.

#### Struttura della solution

Ogni microservizio segue la stessa struttura a quattro progetti:

| Progetto | Responsabilità |
|---|---|
| `*.Domain` | Entità, value object, enum, interfacce dei repository. Zero dipendenze da framework. |
| `*.Application` | Servizi applicativi, DTO, validator FluentValidation, `ServiceCollectionExtensions` per DI. Dipende solo dal Domain e da `Shared.Application`. |
| `*.Infrastructure` | Client MongoDB nativo, BSON class map (`DocumentMap`, `UserMap`), implementazioni dei repository, Unit of Work, `JwtTokenService` (solo Identity), `ServiceCollectionExtensions`. Dipende da `Domain` e da `Shared.Infrastructure`. |
| `*` (API) | Host ASP.NET Core, `Program.cs`, Controller, middleware, configurazione Serilog/JWT/Versioning. |

I progetti `Shared.*` contengono ciò che è realmente condiviso: tipi base, `MongoGenericRepository<T>`, `MongoUnitOfWork`, `JwtOptions`, `MongoOptions`, `GlobalExceptionHandler`, `FilterPagingDto`, configurazione di API versioning.

#### Separazione dei componenti

- Il **Domain** espone solo interfacce e tipi di dominio: non referenzia `MongoDB.Driver`, né ASP.NET Core.
- L'**Application** conosce il Domain e orchestra i casi d'uso; non conosce la tecnologia di persistenza.
- L'**Infrastructure** è l'unico layer a conoscere MongoDB (driver nativo), la serializzazione BSON e l'emissione dei token JWT.
- L'**API** è sottile: i controller delegano subito ai servizi applicativi e si occupano solo di HTTP, autorizzazione e mapping ai codici di risposta.

#### Dependency Injection

La DI è configurata tramite extension method `AddXxxServices(IServiceCollection)` in ogni layer:

- `WAPIDocument.Application.ServiceCollectionExtensions` → registra `IDocumentService`, i validator FluentValidation (auto-discovery via `AddValidatorsFromAssembly`).
- `WAPIDocument.Infrastructure.ServiceCollectionExtensions` → registra `IMongoClient` (singleton), `IMongoDatabase` (scoped), `IUnitOfWork` (scoped), `IDocumentRepository`, invoca `DocumentMap.Register()` per la BSON mapping.
- `WAPIIdentity.Application.ServiceCollectionExtensions` → registra `IUserService`, `ILoginService` e validator.
- `WAPIIdentity.Infrastructure.ServiceCollectionExtensions` → registra `IMongoClient`, `IMongoDatabase`, `IUnitOfWork`, `IUserRepository`, `IJwtTokenService`, binding di `JwtOptions`, invoca `UserMap.Register()`.
- `Shared.Application` → registra il global exception handler e l'API versioning.

Il `Program.cs` di ciascun microservizio richiama questi extension method.

#### Logging, tracing ed error handling

- **Logging**: **Serilog** (`Serilog.AspNetCore`) con logging strutturato e due sink:
  - *Console* per tutti gli ambienti.
  - *MongoDB* (collection `logevents` nel database `logs`) per la persistenza centralizzata, filtrata per source context del solo microservizio corrente (es. `StartsWith(SourceContext, 'WAPIDocument.')`).
  - Ogni servizio arricchisce i log con la proprietà `Application` (`WAPIDocument` / `WAPIIdentity`), facilitando il filtraggio trasversale.
- **Tracing distribuito**: non è attualmente configurato un provider (es. OpenTelemetry). L'architettura è predisposta a integrarlo senza impatto sul dominio.
- **Error handling**: `GlobalExceptionHandler` (in `Shared.Application`, basato su `IExceptionHandler` di ASP.NET Core 8) intercetta le eccezioni non gestite e le mappa in `ProblemDetails`:
  - `KeyNotFoundException` → `404 Not Found`
  - `UnauthorizedAccessException` → `401 Unauthorized`
  - `ArgumentException` / `ValidationException` / `InvalidOperationException` → `400 Bad Request` (con il messaggio dell'eccezione nel campo `detail`)
  - qualsiasi altra eccezione → `500 Internal Server Error`

#### Versioning API

Il versioning è gestito tramite **Asp.Versioning** (Microsoft), configurato centralmente in `Shared.Application.ServiceCollectionExtensions.RegisterApiVersioning()`:

- Versione di default: `1.0`.
- Strategia di routing **URL segment-based**: `api/v{version:apiVersion}/...` (es. `api/v1/documents`, `api/v1/auth/login`).
- `ReportApiVersions = true` aggiunge l'header `api-supported-versions` alle risposte.

### 3. Modellazione dati (MongoDB)

La persistenza è affidata a **MongoDB** tramite il driver ufficiale **MongoDB.Driver** (3.8.0), senza EF Core. La serializzazione BSON è configurata con `BsonClassMap` (registrazione statica tramite `DocumentMap.Register()` e `UserMap.Register()`): l'`Id` è mappato su `_id` con `StringObjectIdGenerator` e `StringSerializer(BsonType.ObjectId)`.

Per l'esecuzione delle applicazioni occorre configurare 3 database con le relative collection:

- database `identity` → collection `users` (per `WAPIIdentity`)
- database `documents` → collection `documents` (per `WAPIDocument`)
- database `logs` → collection `logevents` (per i log di Serilog, condivisa tra i due servizi)

#### Modelli principali

**`Document` (aggregate root)**

Implementa `IEntity<string>`, `IDocument`, `IAudit`. Incapsula la logica di validazione, transizione di stato, generazione e collegamento di documenti.

Campi principali:

- `Id` (string, MongoDB ObjectId) – chiave primaria, generata automaticamente.
- `CreatedAtUtc`, `UpdatedAtUtc` – timestamp di audit.
- `Version` (long) – contatore per la concorrenza ottimistica.
- `Number` (string) – identificativo logico del documento, impostato a `Guid.NewGuid().ToString()` alla creazione.
- `Date` (DateTime UTC) – data del documento, normalizzata a mezzanotte UTC.
- `Customer` – value object embedded (`Customer`).
- `Currency` – codice valuta ISO.
- `Type` – enum `DocumentType` (`Quote = 0`, `Proforma = 1`, `SalesOrder = 2`).
- `Status` – enum `DocumentStatus` (`Draft`, `Ready`, `Sent`, `Approved`, `Rejected`).
- `DocumentLines` – collezione embedded di righe documento (`DocumentLine`); l'assegnazione ricalcola automaticamente `Total`.
- `Total` (decimal) – ricalcolato automaticamente come somma dei `Total` di ogni riga, arrotondato a 2 decimali.
- `LinkedDocuments` – collezione di `DocumentLink` (relazioni con altri documenti).

**`User`**

Implementa `IEntity<string>`, `IAudit`. Campi: `Id`, `Email`, `Password`, `IsActive`, `Roles` (`IList<RoleType>`), `CreatedAtUtc`, `UpdatedAtUtc`, `Version`.

**`Customer` (value object embedded)**

In `WAPIDocument.Domain.Entities.TaxEntities`, implementa `ITaxEntity`. Campi: `Name`, `Email`, `VatNumber`, `Address`. Espone `Clone()` per la copia difensiva in `GenerateFrom`.

**`DocumentLine` (value object embedded)**

Rappresenta una riga del documento: `Description`, `Quantity`, `UnitPrice`, `Total` (calcolato automaticamente alla set di `Quantity` o `UnitPrice`). Espone `IsValid()` (descrizione non vuota, quantità e prezzo > 0) e `Clone()`.

#### Relazioni tra documenti

Le relazioni tra `Quote`, `Proforma` e `SalesOrder` sono modellate tramite `DocumentLink`:

- `TargetDocumentId` – ID del documento collegato.
- `Type` – enum `DocumentLinkType`: `System` (link generato automaticamente da `GenerateFrom`) o `User` (link manuale tramite endpoint `/attachments`).
- I link vivono come collezione embedded (`LinkedDocuments`) sull'aggregato, con riferimento per ID al documento collegato (referencing).
- I link `User` sono **bidirezionali**: entrambi i documenti vengono aggiornati in un'unica transazione MongoDB.

#### Stati del documento

Il ciclo di vita è governato da `DocumentStatus`:

| Stato | Significato | Transizioni ammesse |
|---|---|---|
| `Draft` | In lavorazione, completamente modificabile | → `Ready` |
| `Ready` | Validato (tutti i campi obbligatori presenti), ancora modificabile | → `Sent` |
| `Sent` | Inviato al cliente; diventa immutabile | → `Approved`, `Rejected` |
| `Approved` | Accettato; consente di generare il documento successivo | stato finale |
| `Rejected` | Rifiutato | stato finale |

Le transizioni sono centralizzate nel metodo `UpdateStatus(...)` dell'aggregato. La transizione verso `Ready` o `Sent` richiede che il documento sia validato (valuta presente, cliente con `Name` e `VatNumber`, almeno una riga valida). Gli aggiornamenti (`Update(...)`) sono consentiti solo in stato `Draft` e `Ready`; in stato `Ready` la modifica ri-esegue la validazione per mantenere il documento valido. La cancellazione è consentita solo in stato `Draft` e `Ready`.

Il campo `Status = Draft` non può essere impostato tramite `UpdateStatusAsync` (validato da `DocumentChangeStatusValidator`).

#### Strategie di embedding vs referencing

- **Embedding**: `Customer` (snapshot fiscale al momento dell'emissione) e `DocumentLines` sono sempre letti insieme al documento → embedding minimizza i round-trip e garantisce la consistenza storica.
- **Referencing**: `LinkedDocuments` usa solo `TargetDocumentId` per i collegamenti tra documenti distinti, evitando duplicazione e rischi di disallineamento.

#### Concorrenza ottimistica

Gestita direttamente in `MongoGenericRepository<T>.UpdateAsync(...)`:

1. Si incrementa `Version` in memoria.
2. Il filtro MongoDB include `AND(Id == id, Version == oldVersion)`.
3. Se `MatchedCount == 0` la versione è cambiata: si ripristina la versione in memoria e si solleva `InvalidOperationException` (mappata a `400 Bad Request` dal `GlobalExceptionHandler`).

#### Transazioni MongoDB multi-documento

Le operazioni che coinvolgono più documenti usano `MongoUnitOfWork` (sessione MongoDB con `ReadConcern.Majority` / `WriteConcern.WMajority`):

- **`DeleteByIdAsync`**: elimina il documento e aggiorna i `LinkedDocuments` di tutti i documenti collegati (pulizia referenze), in una singola transazione con rollback automatico in caso di errore.
- **`GenerateFromAsync`**: inserisce il nuovo documento generato e aggiorna i `LinkedDocuments` del documento sorgente, in transazione.
- **`AttachAsync`**: aggiorna bidirezionalmente i `LinkedDocuments` di entrambi i documenti collegati manualmente, in transazione.

#### Ricerca e paginazione

`FindPagedByFilterAsync` supporta i filtri combinabili:

- `DocumentTypes` (lista di `DocumentType`) – filtra per tipologia.
- `DocumentStatuses` (lista di `DocumentStatus`) – filtra per stato.
- `CustomerName` (string) – ricerca case-insensitive su `Customer.Name` (contains).
- `PlainText` (string, ereditato da `FilterPagingDto`) – ricerca su `Number` (contains) oppure su `Date` (match esatto se il testo è una data valida).
- `SortBy` / `SortDirection` (ereditati da `FilterPagingDto`) – ordinamento dinamico sul campo indicato.
- `Page` / `PageSize` – paginazione (default: pagina 1, dimensione 20).

La validazione dei parametri di paginazione e filtro avviene tramite `DocumentFindPagedByFilterRequestValidator` e `FilterPagingDtoValidator`, applicati via `[AutoValidationActionFilter]` sull'endpoint `GET /documents`.

---

## Stack tecnologico

| Area | Tecnologia |
|---|---|
| Runtime | .NET 8.0 |
| API | ASP.NET Core Web API |
| Persistenza | MongoDB – driver nativo `MongoDB.Driver` 3.8.0 + BSON class map |
| Autenticazione | JWT (HMAC-SHA256, `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.x) |
| Validation | FluentValidation 8.6.3 |
| Logging | Serilog (`Serilog.AspNetCore` 8.0.3) – sink Console + MongoDB |
| Error handling | `IExceptionHandler` + ProblemDetails (ASP.NET Core 8) |
| API versioning | Asp.Versioning (URL segment) |
| Documentazione API | Swashbuckle / Swagger (con supporto Bearer token) |
| Test | xUnit 2.9.2 + Moq 4.20.x + coverlet |

---

## Endpoint API

### WAPIIdentity — https://localhost:7205

| Metodo | Route | Auth | Descrizione |
|---|---|---|---|
| `POST` | `/api/v1/auth/login` | Anonimo | Autenticazione, restituisce JWT |
| `POST` | `/api/v1/users/register` | `Admin` | Registrazione nuovo utente |

### WAPIDocument — https://localhost:7273

| Metodo | Route | Auth | Descrizione |
|---|---|---|---|
| `POST` | `/api/v1/documents` | `Editor`, `Admin` | Crea un documento |
| `GET` | `/api/v1/documents/{id}` | `Viewer`, `Editor`, `Admin` | Legge un documento per ID |
| `GET` | `/api/v1/documents` | `Viewer`, `Editor`, `Admin` | Ricerca paginata e filtrata |
| `PUT` | `/api/v1/documents/{id}` | `Editor`, `Admin` | Aggiorna un documento (`Draft` / `Ready`) |
| `PUT` | `/api/v1/documents/{id}/status` | `Editor`, `Admin` | Avanza lo stato del documento |
| `DELETE` | `/api/v1/documents/{id}` | `Editor`, `Admin` | Elimina un documento (`Draft` / `Ready`) |
| `POST` | `/api/v1/documents/{id}/generation` | `Editor`, `Admin` | Genera un nuovo documento da uno esistente |
| `POST` | `/api/v1/documents/{id}/attachments` | `Editor`, `Admin` | Collega manualmente due documenti |

---

## Configurazione

Il file `appsettings.json` di ciascun servizio espone le sezioni:

- **`Mongo`**: `ConnectionString` (default `mongodb://localhost:27017`), `Database` (`identity` / `documents`).
- **`Jwt`**: `Issuer`, `Audience` (entrambi `http://localhost:7205`), `SigningKey` (256-bit hex), `ExpirationMinutes` (default `60`).
- **`Serilog`**: configurazione sink Console e MongoDB con filtro per source context del servizio corrente.
- **`Cors`**: `AllowedOrigins` – elenco delle origini autorizzate a chiamare le API dal browser (default `https://localhost:7150` e `http://localhost:5150`, cioè il client Blazor WebAssembly). La policy CORS di default ammette qualsiasi header e metodo solo per queste origini.

---

## Progetti di test

La solution include quattro progetti di test che coprono i livelli Application e Api di entrambi i microservizi:

- `WAPIDocument.Application.Tests` – servizi (`DocumentServiceTests`, `DocumentServiceWorkflowTest`) e validator (`DocumentCreateRequestValidatorTests`, `DocumentUpdateRequestValidatorTests`, `DocumentCreateUpdateRequestDocumentLineValidatorTests`).
- `WAPIDocument.Api.Tests` – controller (`DocumentsControllerTests`).
- `WAPIIdentity.Application.Tests` – servizi (`UserServiceTests`, `LoginServiceTests`) e validator (`LoginRequestValidatorTests`, `RegisterUserRequestValidatorTests`).
- `WAPIIdentity.Api.Tests` – controller (`AuthControllerTests`, `UsersControllerTests`).

Tutti i test usano **xUnit** + **Moq** per il mocking delle dipendenze. I validator FluentValidation vengono mockati come "successo" nei test di servizio, così i test si concentrano sulla logica applicativa e di dominio. La copertura è raccolta tramite **coverlet**.

---

## Client UI (Blazor WebAssembly)

Client web **Blazor WebAssembly Standalone** (.NET 8) nella cartella `UI/`, con solution dedicata `WAPIDocManager.UI.slnx`. Si autentica su `WAPIIdentity` (login JWT) e offre l'interfaccia per tutti gli endpoint di `WAPIDocument`, più la registrazione utenti riservata agli `Admin`.

### Struttura

Il client è **un solo progetto**, organizzato a *vertical slice*: ogni funzionalità sta in una cartella sotto
`Features/` con tutto ciò che le serve — pagine, componenti, entità (ciò che torna dalle API), modelli di form,
chiamate HTTP e contratti wire. Fuori da `Features/` sta soltanto ciò che **due o più** funzionalità usano davvero.

```
UI/
├── Directory.Build.props             (proprietà di build comuni: net8.0, nullable, analizzatori)
├── Directory.Packages.props          (versioni dei pacchetti NuGet, gestione centralizzata)
├── .editorconfig                     (stile di codifica: template ufficiale SDK + personalizzazioni)
├── Dockerfile / nginx.conf / docker-compose.yml   (esecuzione del client in container)
├── WAPIDocManager.UI.slnx
├── WAPIDocManager.UI/
│   ├── Features/
│   │   ├── Auth/         Views/Pages · Models · Services · Contracts      (login, sessione)
│   │   ├── Documents/    Views/{Pages,Components} · ViewModels · Entities · Models · Services · Contracts
│   │   │                 + DocumentPermissions · DocumentListState · DocumentLabels
│   │   ├── Users/        Views/Pages · Models · Services · Contracts      (registrazione, solo Admin)
│   │   └── Home/         Views/Pages
│   ├── Shared/           Api · Authentication · Validation · Components · Formatting
│   │                     · Forms · Localization · Navigation
│   ├── Layout/ · Resources/ · wwwroot/
│   └── Program.cs · App.razor · _Imports.razor · SharedResource.cs
└── Tests/WAPIDocManager.UI.Tests/    (unico progetto di test, stessa struttura)
```

| Cartella dello slice | Contenuto |
|---|---|
| `Views/Pages/` · `Views/Components/` | markup e code-behind: pagine routabili e componenti della funzionalità |
| `ViewModels/` | logica delle pagine che ne hanno (MVVM solo dove serve: le pagine sottili come `DocumentCreate` o `Login` non ne hanno), testata senza Blazor |
| `Entities/` | ciò che torna dalle API già trasformato per l'app: `Document`, `Customer`, `PagedResult`, gli enum e `DocumentRules` (regole di stato speculari al server) |
| `Models/` | modelli di form e di filtro, senza attributi: `DocumentEditModel`, `DocumentFilter`, `LoginModel`, `RegisterUserModel` |
| `Validators/` | regole dei form (FluentValidation), una classe per modello; i messaggi sono chiavi di `ValidationKeys`, tradotte alla resa |
| `Services/` | interfaccia + typed `HttpClient` verso le API della funzionalità, mapper e query string builder |
| `Contracts/` | forme di trasporto (DTO wire) con gli operatori di conversione espliciti |

Il punto di ingresso per capire una funzionalità è il commento in testa alla sua classe di registrazione
(`Features/<Area>/<Area>FeatureRegistration.cs`), richiamata da `Program.cs` dopo `AddApiInfrastructure`.

La validazione dei form usa **FluentValidation** con **Blazilla** come ponte verso `EditForm`: ogni form monta
`<FluentValidator />`, che risolve dalla DI il validator registrato dalla registrazione del proprio slice. Gli oggetti
annidati e le collezioni sono coperti da `SetValidator` e `RuleForEach` (righe del documento e dati cliente). I
messaggi restano chiavi di `ValidationKeys`, tradotte da `Shared/Forms/FieldValidationMessage`. Costo misurato sul
bundle: **+244 KB** compressi, verificati funzionanti anche nel publish con trimming attivo.

Il pattern MVVM usa **CommunityToolkit.Mvvm**: i ViewModel derivano da `ObservableObject` e notificano i cambiamenti
di proprietà; `Shared/Mvvm/MvvmComponentBase<TViewModel>` — una trentina di righe scritte qui — inietta il ViewModel
nella pagina (`@inherits MvvmComponentBase<DocumentListViewModel>`) e traduce le notifiche in `StateHasChanged`.
Non è stato adottato Blazing.Mvvm, che farebbe lo stesso ma mettendo un pacchetto di terze parti nella classe base di
tutte le pagine. Costo nel bundle, misurato con due publish a confronto: **+12 KB** compressi.

Nello slice Documenti convivono **tre forme** dello stesso documento — `Contracts/DocumentResponse` (trasporto),
`Entities/Document` (lettura), `Models/DocumentEditModel` (form) — documentate in testa a `DocumentResponse.cs`.
`Documents` non è divisa per caso d'uso (creazione, modifica, …) perché quelle pagine condividono form, modello,
entità e servizio: la divisione naturale, se servirà, è per sotto-area.

In `Shared/` un tipo entra quando compare il **secondo** utilizzatore; finché ne ha uno solo resta nello slice
(per questo `PagedResult` e `PageDto` stanno in `Features/Documents`).

### Funzionalità

- **Login** su `POST /api/v1/auth/login`; il token JWT viene salvato nel `sessionStorage` e i ruoli sono letti dal payload del token. Le pagine e le azioni sono mostrate in base al ruolo (`Viewer` lettura, `Editor` scrittura, `Admin` completo).
- **Documenti**: ricerca paginata con filtri (tipologie, stati, cliente, numero/data), ordinamento; creazione; modifica (solo `Draft`/`Ready`); avanzamento di stato (`Draft → Ready → Sent → Approved/Rejected`); generazione di un nuovo documento da uno esistente; collegamento manuale bidirezionale; eliminazione con conferma; elenco dei documenti collegati.
- **Valuta**: il client gestisce solo Euro – invia sempre `Currency = "EUR"` e mostra gli importi in `€`.
- **Design system**: Bootstrap 5.3.8 (solo CSS, in `wwwroot/lib/bootstrap`) + un unico stylesheet `wwwroot/css/app.css` con i design token e gli stili di tutti i componenti (nessun CSS isolato per componente). Temi **light** e **dark** tramite `data-bs-theme`, scelta salvata nel browser (default: tema di sistema).
- **Localizzazione**: `IStringLocalizer` con `Resources/SharedResource.resx` (italiano, predefinito) e `SharedResource.en.resx` (inglese); la lingua si cambia dalla top bar (ricarica della pagina). Gli errori restituiti dalle API sono mostrati così come arrivano (inglese) sotto un titolo localizzato.

### Configurazione e avvio

- `UI/WAPIDocManager.UI/wwwroot/appsettings.json` → sezione `Api`: `IdentityBaseUrl` (`https://localhost:7205/`) e `DocumentBaseUrl` (`https://localhost:7273/`).
- Le API devono autorizzare l'origine del client nella sezione `Cors:AllowedOrigins` (vedi [Configurazione](#configurazione)).
- Avvio: eseguire `WAPIIdentity` e `WAPIDocument` (profilo `https`), poi `dotnet run --project UI/WAPIDocManager.UI --launch-profile https` e aprire `https://localhost:7150`.

### Test del client

Un unico progetto, `Tests/WAPIDocManager.UI.Tests`, con la stessa struttura del sorgente (123 test):

- `Features/Documents` – regole di stato e completezza, permessi per ruolo e stato, validazione e precompilazione dei
  modelli di form, conversioni dei contratti, `DocumentApiService` con `HttpMessageHandler` fittizio (URL, metodi,
  query string con liste, body JSON, enum numerici, date UTC, valuta EUR) e i ViewModel delle quattro pagine.
- `Features/Auth` – login, lettura dei ruoli dal JWT, conversione `LoginResponse` → `UserSession`.
- `Features/Users` – registrazione utenti e conversione del relativo contratto.
- `Shared/Api` – lettura delle risposte ProblemDetails → `ApiException`, `BearerTokenHandler` (token scaduto, 401, 403).
- `Shared/Authentication` – sessione utente e persistenza nel `sessionStorage`.
- `Shared/Validation` – `ModelValidator` (validazione del grafo, righe documento) e attributi personalizzati.
- `TestSupport/` – fake riusabili: `HttpMessageHandler`, `IJSRuntime`, `TimeProvider`, generatore di JWT.

### Configurazione di build centralizzata

Tutti i file sono a livello della solution client (`UI/`) e valgono solo per essa: il backend non è coinvolto.

| File | Contenuto | Quando si tocca |
|---|---|---|
| `UI/Directory.Build.props` | `TargetFramework`, `Nullable`, `ImplicitUsings` e le proprietà dell'analisi statica, comuni a tutti i progetti | per una proprietà che deve valere ovunque |
| `UI/Tests/WAPIDocManager.UI.Tests/*.csproj` | proprietà e pacchetti del progetto di test (`IsTestProject`, `using Xunit` implicito, `Microsoft.NET.Test.Sdk`, `xunit`, `Moq`, `coverlet.collector`) | il progetto di test è uno solo, quindi non serve un `Directory.Build.props` dedicato |
| `UI/Directory.Packages.props` | **versioni** di tutti i pacchetti NuGet (`ManagePackageVersionsCentrally`) | per aggiungere un pacchetto o aggiornare una versione |
| `UI/.editorconfig` | stile di codifica e severità delle regole | per cambiare una convenzione o motivare una soppressione |

Per aggiungere un pacchetto: `<PackageVersion Include="Nome" Version="x.y.z" />` in `Directory.Packages.props`, poi `<PackageReference Include="Nome" />` (senza versione) nel `.csproj` che lo usa. Una versione lasciata nel `.csproj` fa fallire il restore con `NU1008`.

### Stile di codifica e analisi statica

`UI/.editorconfig` parte dal template ufficiale dell'SDK (`dotnet new editorconfig`, SDK 8.0.425); le poche voci modificate sono marcate `[PERSONALIZZATO]` con il motivo: fine riga `lf`, newline finale, namespace file-scoped (severità `warning`), niente riga vuota tra i gruppi di `using`, sezioni per `.razor`, i file MSBuild, `.resx` e gli asset di `wwwroot`.

Analizzatori attivi su tutti i progetti:

| Fonte | Diagnostiche | Configurazione |
|---|---|---|
| Analizzatori .NET dell'SDK | `CAxxxx` | `EnableNETAnalyzers`, `AnalysisLevel=latest`, `AnalysisMode=Recommended` in `Directory.Build.props` |
| Regole di stile | `IDExxxx` | `EnforceCodeStyleInBuild=true` + `.editorconfig` |
| SonarAnalyzer.CSharp | `Sxxxx` | `GlobalPackageReference` in `Directory.Packages.props` (vale per tutti i progetti) |
| xunit.analyzers | `xUnit1xxx`, `xUnit2xxx` | incluso nel metapacchetto `xunit`, nessuna configurazione |
| Moq.Analyzers | `Moq1xxx` | `PackageReference` nei tre progetti di test che usano Moq |

I warning **non** sono trattati come errori, ma la solution compila a **0 warning**: una segnalazione nuova va corretta oppure motivata con una riga di severità nell'`.editorconfig` (in fondo al file ci sono le soppressioni attuali, ognuna con il proprio commento). Non sono stati inclusi StyleCop (regole di stile in conflitto con `EnforceCodeStyleInBuild`), Roslynator e Meziantou (in larga parte sovrapposti a Sonar).

### Esecuzione del client in Docker

Il client Blazor WASM pubblica file statici, quindi l'immagine è in due stadi: build con l'SDK .NET 8, esecuzione con `nginx:alpine`. Le due API e MongoDB **non** sono containerizzate: restano in esecuzione sulla macchina.

```bash
cd UI
docker compose up --build -d     # http://localhost:8080
docker compose down
```

Gli indirizzi delle API non sono compilati nell'immagine: `docker-entrypoint.sh` riscrive `wwwroot/appsettings.json` all'avvio del container a partire da `API_IDENTITY_BASE_URL` e `API_DOCUMENT_BASE_URL` (impostate in `docker-compose.yml`), quindi la stessa immagine può puntare ad ambienti diversi senza essere ricompilata. Sono indirizzi usati dal **browser**, non dal container: `localhost` è la macchina di chi apre la pagina.

Perché il client in container funzioni servono due condizioni:

1. `http://localhost:8080` deve essere presente in `Cors:AllowedOrigins` di **entrambe** le API, altrimenti il browser blocca ogni chiamata (in UI compare l'errore "servizio non raggiungibile");
2. il certificato di sviluppo ASP.NET deve essere fidato nel browser, perché le API rispondono in `https`.
