# WAPIDocManager

Sistema di gestione documenti commerciali (preventivi, proforme e ordini di vendita) realizzato in **.NET 8** secondo un'architettura a microservizi, con persistenza su **MongoDB** e autenticazione basata su **JWT**.

La solution è composta da due microservizi indipendenti (`WAPIIdentity` e `WAPIDocument`) che condividono tre librerie trasversali (`Shared.Domain`, `Shared.Application`, `Shared.Infrastructure`), oltre a quattro progetti di test automatizzati.

---

## Parte 1 – Analisi e architettura

### 1. Architettura a microservizi

La soluzione è suddivisa in due microservizi principali, pensati per essere eseguiti, scalati e versionati in modo indipendente. Ogni microservizio possiede il proprio database MongoDB dedicato ed è strutturato secondo i principi della Clean Architecture (Domain / Application / Infrastructure / API).

```
WAPIDocManager.sln
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
  - Espone CRUD e operazioni di dominio (generazione documento da documento esistente, cambio di stato, ricerca paginata e filtrata).
- **Modalità di comunicazione**
  - **REST** sincrono (endpoint `DocumentsController`, route `api/v{version:apiVersion}/documents`).
  - Nessuna comunicazione esterna verso altri microservizi.
- **Sicurezza**
  - Tutti gli endpoint sono protetti da `[Authorize]` con controllo di ruolo:
    - `Viewer` e `Admin` possono leggere.
    - `Editor` e `Admin` possono creare, aggiornare, cambiare stato ed eliminare.
  - Validazione input tramite **FluentValidation**.
  - Verifica del JWT con relativa scadenza.

### 2. Backend .NET

#### Versione di .NET utilizzata

Tutti i progetti della solution hanno come target framework **.NET 8.0** (`net8.0`).

#### Pattern architetturali

La solution adotta un mix di pattern consolidati:

- **Clean Architecture** (Onion Architecture) su ciascun microservizio: la dipendenza punta sempre dall'esterno verso il Domain. Il progetto `Domain` non ha dipendenze da Infrastructure o Api.
- **Domain-Driven Design (DDD) tattico**: entità con identità (`IEntity<T>`), value object immutabili (`Customer`, `DocumentLine`), aggregate root (`Document`) che incapsula regole di invariante (validazione, transizioni di stato, generazione di documenti collegati).
- **Repository Pattern** + **Unit of Work**: `IDocumentRepository`, `IUserRepository`, `IUnitOfWork` astraggono l'accesso al database; le implementazioni vivono nei rispettivi layer `*.Infrastructure`.
- **Service Layer**: la logica applicativa è orchestrata da servizi (`DocumentService`, `UserService`, `LoginService`) che ricevono DTO, invocano il dominio e persistono tramite repository.
- **DTO + Validation**: ogni operazione API accetta un Request DTO validato tramite FluentValidation e restituisce un Response DTO. Il dominio non viene mai esposto direttamente.
- L'implementazione fa leva su un Service Layer tradizionale, più semplice da evolvere e sufficiente rispetto alla complessità attuale del dominio.

#### Struttura della solution (.sln)

Ogni microservizio segue la stessa struttura a quattro progetti:

| Progetto | Responsabilità |
|---|---|
| `*.Domain` | Entità, value object, enum, interfacce dei repository. Zero dipendenze da framework. |
| `*.Application` | Servizi applicativi, DTO, validator FluentValidation, `ServiceCollectionExtensions` per DI. Dipende solo dal Domain e da `Shared.Application`. |
| `*.Infrastructure` | `DbContext` MongoDB EF Core, implementazioni dei repository, Unit of Work, `JwtTokenService` (solo Identity), `ServiceCollectionExtensions`. Dipende da `Domain` e da `Shared.Infrastructure`. |
| `*` (API) | Host ASP.NET Core, `Program.cs`, Controller, middleware, configurazione Serilog/JWT/Versioning. |

I progetti `Shared.*` contengono ciò che è realmente condiviso (tipi base, `BaseRepository`, `JwtOptions`, `MongoOptions`, `GlobalExceptionHandler`, configurazione di API versioning), evitando duplicazione.

#### Separazione dei componenti

- Il **Domain** espone solo interfacce e tipi di dominio: non referenzia `Microsoft.EntityFrameworkCore`, né `MongoDB.Driver`, né ASP.NET Core.
- L'**Application** conosce il Domain e orchestra i casi d'uso; non conosce la tecnologia di persistenza.
- L'**Infrastructure** è l'unico layer a conoscere MongoDB e l'emissione dei token JWT.
- L'**API** è sottile: i controller delegano subito ai servizi applicativi e si occupano solo di HTTP, autorizzazione e mapping ai codici di risposta.

#### Dependency Injection

La DI è configurata tramite extension method `AddXxxServices(IServiceCollection)` in ogni layer:

- `WAPIDocument.Application.ServiceCollectionExtensions` → registra `IDocumentService`, i validator FluentValidation (auto-discovery via `AddValidatorsFromAssembly`).
- `WAPIDocument.Infrastructure.ServiceCollectionExtensions` → registra `DocumentsDbContext`, `IDocumentRepository`, `IUnitOfWork`, opzioni MongoDB.
- `WAPIIdentity.Application.ServiceCollectionExtensions` → registra `IUserService`, `ILoginService` e validator.
- `WAPIIdentity.Infrastructure.ServiceCollectionExtensions` → registra `IdentityDbContext`, `IUserRepository`, `IUnitOfWork`, `IJwtTokenService`, binding di `JwtOptions`.
- `Shared.Application` → registra il global exception handler e l'API versioning.

Il `Program.cs` richiama questi extension method.

#### Logging, tracing ed error handling

- **Logging**: **Serilog** (pacchetto `Serilog.AspNetCore`) con logging strutturato. Sono configurati due sink:
  - *Console* per l'ambiente di sviluppo.
  - *MongoDB* (`logevents` nel database `logs`) per la persistenza centralizzata dei log applicativi.
  - Ogni servizio arricchisce i log con il tag `Application` (`WAPIDocument` / `WAPIIdentity`), facilitando il filtraggio trasversale.
- **Tracing distribuito**: non è attualmente configurato un provider (es. OpenTelemetry). L'architettura è comunque predisposta a integrarlo senza impatto sul dominio, poiché i servizi non si chiamano direttamente tra loro via HTTP.
- **Error handling**: è implementato un `GlobalExceptionHandler` (in `Shared.Application`, basato su `IExceptionHandler` di ASP.NET Core 8) che intercetta le eccezioni non gestite e le mappa in risposte:
  - `KeyNotFoundException` → `404 NotFound`
  - `UnauthorizedAccessException` → `401 Unauthorized`
  - `ArgumentException` / `ValidationException` / `InvalidOperationException` → `400 BadRequest`
  - qualsiasi altra eccezione → `500 InternalServerError`

  Questo garantisce una rappresentazione degli errori uniforme e gestione centralizzata.

#### Versioning API

Il versioning è gestito tramite **Asp.Versioning (Microsoft.AspNetCore.Mvc.Versioning)**, configurato centralmente in `Shared.Application`:

- Versione di default: `1.0`.
- Strategia di routing **URL segment-based**: `api/v{version:apiVersion}/...` (es. `api/v1/documents`, `api/v1/auth/login`).
- L'approccio permette di introdurre in futuro v1.1 / v2 senza impattare i client esistenti, semplicemente affiancando nuovi controller decorati con `[ApiVersion("2.0")]`.

### 3. Modellazione dati (MongoDB)

La persistenza è affidata a **MongoDB** tramite il provider **MongoDB.EntityFrameworkCore** (8.4.1).

#### Modelli principali

**`Document` (entità base)**

L'aggregate root `Document` implementa le interfacce `IDocument` e `IEntity<string>`. Incapsula la logica di validazione, transizione di stato e generazione di documenti collegati.

Campi principali:

- `Id` (string, GUID) – chiave primaria.
- `CreatedAtUtc`, `UpdatedAtUtc`, `Version` – campi di audit e concorrenza ottimistica.
- `Number` – identificativo logico del documento.
- `Date` – data del documento.
- `Customer` – value object embedded (`Customer`).
- `Currency` – codice valuta ISO.
- `Type` – enum `DocumentType` (`Quote = 0`, `Proforma = 1`, `SalesOrder = 2`).
- `Status` – enum `DocumentStatus` (`Draft`, `Ready`, `Sent`, `Approved`, `Rejected`).
- `DocumentLines` – collezione embedded di righe documento (value object `DocumentLine`).
- `Total` – calcolato dall'aggregato a partire dalle righe.
- `LinkedDocuments` – collezione di `DocumentLink` che descrive le relazioni con altri documenti (fonte/destinazione).

**Quote / Proforma / SalesOrder**

Le tre tipologie di documento condividono la stessa entità `Document` e si distinguono attraverso la proprietà `Type` (`DocumentType`). Questo approccio semplifica lo storage, mantiene un'unica collezione indicizzabile e centralizza la logica nel Domain.

**`Customer` (value object embedded)**

Entità fiscale in `WAPIDocument.Domain.Entities.TaxEntities`, implementa `ITaxEntity`. Contiene `Name`, `Email`, `VatNumber`, `Address`.

**`DocumentLine` (value object embedded)**

Rappresenta una riga del documento: `Description`, `Quantity`, `UnitPrice`, `Total` calcolato. Include un metodo `IsValid()` usato dall'aggregato per rifiutare righe malformate prima di consentire transizioni di stato (es. `Draft → Ready`).

#### Relazioni tra documenti

Le relazioni tra `Quote`, `Proforma` e `SalesOrder` sono modellate esplicitamente tramite `DocumentLink`:

- `DocumentLink` contiene `TargetDocumentId`, `DocumentType` di destinazione e `LinkType` (enum `DocumentLinkType`, valori `System` e `User`).
  - `System`: il link è generato automaticamente dal flusso di `GenerateFrom(...)` (es. generazione di una Proforma a partire da un Quote).
  - `User`: il link è manuale, creato dall'utente tramite endpoint di "attach".
- I link vivono come collezione embedded (`LinkedDocuments`) sull'aggregato `Document` ma con riferimento al documento collegato.

#### Stati del documento

Il ciclo di vita del documento è governato dall'enum `DocumentStatus`:

- `Draft` – documento in lavorazione, completamente modificabile.
- `Ready` – documento pronto, ancora modificabile ma validato.
- `Sent` – documento inviato al cliente; diventa immutabile.
- `Approved` – documento accettato dal cliente; consente di generare il documento successivo nella catena (es. Proforma → SalesOrder).
- `Rejected` – documento rifiutato; stato terminale.

Le transizioni sono centralizzate nel metodo `UpdateStatus(...)` dell'aggregato `Document`, rifiutando transizioni non ammesse. Gli `Update(...)` vengono consentiti solo negli stati `Draft` e `Ready`, garantendo l'integrità storica dei documenti già inviati.

#### Strategie di embedding vs referencing

- **Embedding** per i dati fortemente coesi con il documento e che vengono sempre letti insieme a esso:
  - `Customer` (snapshot del cliente al momento dell'emissione).
  - `DocumentLines` (righe del documento).
  
  Questa scelta minimizza il numero di round-trip verso MongoDB e garantisce la consistenza storica: le modifiche anagrafiche a un cliente non riscrivono documenti già emessi.
- **Referencing** è utilizzato in `LinkedDocuments` (catena di collegamenti) tramite `TargetDocumentId` per i collegamenti tra documenti distinti (Quote ↔ Proforma ↔ SalesOrder): si mantiene solo l'ID, evitando duplicazione del documento referenziato e i rischi di disallineamento.

#### Concorrenza

La concorrenza ottimistica è gestita tramite un campo `Version` (row version) presente sull'aggregato `Document`. In fase di `SaveChanges`, EF Core verifica che la versione letta corrisponda a quella sul database: in caso contrario viene sollevata una `DbUpdateConcurrencyException`.

#### Indici

La collezione `documents` è configurata in `DocumentsDbContext` con:

- **Chiave primaria** su `Id` (string GUID).

La collezione `logs.logevents` (scritta da Serilog) è separata e non concorre con le query sui documenti.

---

## Stack tecnologico

| Area | Tecnologia |
|---|---|
| Runtime | .NET 8.0 |
| API | ASP.NET Core Web API |
| Persistenza | MongoDB (driver `MongoDB.EntityFrameworkCore` 8.4.1) |
| Autenticazione | JWT (HMAC-SHA256, `System.IdentityModel.Tokens.Jwt`) |
| Validation | FluentValidation |
| Logging | Serilog (sink Console + MongoDB) |
| Error handling | `IExceptionHandler` + ProblemDetails |
| API versioning | Asp.Versioning (URL segment) |
| Documentazione API | Swashbuckle / Swagger |
| Test | xUnit + Moq |

## Progetti di test

La solution include quattro progetti di test che coprono i livelli Application e Api di entrambi i microservizi:

- `WAPIDocument.Application.Tests` – servizi (`DocumentServiceTests`, `DocumentServiceWorkflowTest`) e validator.
- `WAPIDocument.Api.Tests` – controller (`DocumentsControllerTests`).
- `WAPIIdentity.Application.Tests` – servizi (`UserServiceTests`, `LoginServiceTests`) e validator.
- `WAPIIdentity.Api.Tests` – controller (`AuthControllerTests`, `UsersControllerTests`).
