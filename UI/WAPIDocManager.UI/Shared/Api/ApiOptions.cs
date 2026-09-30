namespace WAPIDocManager.UI.Shared.Api;

/// Indirizzi base dei due servizi a cui il client si rivolge.
/// In WebAssembly la configurazione non è compilata nell'eseguibile: è un file che il browser scarica, quindi non
/// può contenere segreti. I file JSON non ammettono commenti, ed è la ragione per cui la configurazione è
/// documentata qui e non accanto ai valori. In sviluppo l'autenticazione risponde sulla porta 7205 e i documenti
/// sulla 7273.
/// Ogni origine da cui il client viene servito deve essere autorizzata fra le origini consentite di ENTRAMBI i
/// servizi, altrimenti il browser blocca le chiamate e in interfaccia si vede un generico "servizio non
/// raggiungibile".
public class ApiOptions
{
    public const string SectionName = "Api";

    /// Indirizzo del servizio di autenticazione. Lo slash finale viene aggiunto se manca.
    public string IdentityBaseUrl { get; set; } = string.Empty;

    /// Indirizzo del servizio documenti. Lo slash finale viene aggiunto se manca.
    public string DocumentBaseUrl { get; set; } = string.Empty;
}
