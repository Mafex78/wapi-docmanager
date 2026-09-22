#!/bin/sh
# -------------------------------------------------------------------------------------------------------------
# Scrive wwwroot/appsettings.json all'avvio del container, prima che nginx parta.
#
# In Blazor WebAssembly la configurazione non è incorporata nell'eseguibile: è un file che il browser scarica
# (vedi WAPIDocManager.UI/Shared/Api/ApiOptions.cs). Riscriverlo qui permette di usare la STESSA immagine
# con API diverse, cambiando solo le variabili d'ambiente, senza ricompilare.
#
# Variabili (con i valori di sviluppo come default):
#   API_IDENTITY_BASE_URL   indirizzo di WAPIIdentity   (default https://localhost:7205/)
#   API_DOCUMENT_BASE_URL   indirizzo di WAPIDocument   (default https://localhost:7273/)
#
# Sono indirizzi usati dal BROWSER, non dal container: "localhost" è quindi la macchina di chi apre la pagina.
# -------------------------------------------------------------------------------------------------------------
set -e

identity_url="${API_IDENTITY_BASE_URL:-https://localhost:7205/}"
document_url="${API_DOCUMENT_BASE_URL:-https://localhost:7273/}"

cat > /usr/share/nginx/html/appsettings.json <<JSON
{
  "Api": {
    "IdentityBaseUrl": "${identity_url}",
    "DocumentBaseUrl": "${document_url}"
  }
}
JSON

echo "WAPIDocManager UI: Identity=${identity_url} Document=${document_url}"
