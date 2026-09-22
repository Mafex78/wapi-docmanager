# -------------------------------------------------------------------------------------------------------------
# Immagine del solo client Blazor WebAssembly Standalone.
#
# Blazor WASM pubblica file statici (HTML, JS, CSS, .wasm e le .dll dell'app scaricate dal browser): non serve
# un runtime .NET nel container finale, basta un web server. Quindi due stadi: build con l'SDK, esecuzione con nginx.
#
# Le API (WAPIIdentity e WAPIDocument) e MongoDB NON sono in questa immagine: restano in esecuzione sulla macchina.
# Gli indirizzi delle API si passano come variabili d'ambiente (vedi docker-entrypoint.sh), non sono compilati dentro.
#
# Build ed esecuzione (dalla cartella UI/):
#   docker compose up --build -d          poi http://localhost:8080
#   docker compose down
# Oppure senza compose:
#   docker build -t wapidocmanager-ui .
#   docker run --rm -p 8080:80 wapidocmanager-ui
#
# ATTENZIONE: il client servito dal container ha origine http://localhost:8080, che deve comparire in
# Cors:AllowedOrigins di ENTRAMBE le API, altrimenti il browser blocca ogni chiamata.
# -------------------------------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Prima i soli file di progetto: se non cambiano, Docker riusa il layer del restore (build molto più veloci).
# Directory.Build.props e Directory.Packages.props servono al restore: contengono TargetFramework e le versioni.
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY WAPIDocManager.UI/WAPIDocManager.UI.csproj WAPIDocManager.UI/
RUN dotnet restore WAPIDocManager.UI/WAPIDocManager.UI.csproj

# Poi il codice: unico progetto sorgente (il progetto di test non serve all'immagine, è escluso da .dockerignore)
COPY WAPIDocManager.UI/ WAPIDocManager.UI/
RUN dotnet publish WAPIDocManager.UI/WAPIDocManager.UI.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final

# Il publish di Blazor WASM mette il sito completo in wwwroot (index.html, _framework/, css, js, lib)
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# L'immagine nginx esegue tutti gli script in /docker-entrypoint.d prima di avviare il server:
# qui dentro riscriviamo appsettings.json con gli indirizzi delle API presi dalle variabili d'ambiente.
COPY docker-entrypoint.sh /docker-entrypoint.d/40-wapidocmanager-appsettings.sh
RUN chmod +x /docker-entrypoint.d/40-wapidocmanager-appsettings.sh

EXPOSE 80
