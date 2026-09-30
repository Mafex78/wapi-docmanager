namespace WAPIDocManager.UI;

/// Segnaposto delle risorse tradotte condivise: non ha contenuto, serve solo a dare un tipo al traduttore.
/// Deve restare nella radice del progetto. Il nome della risorsa viene costruito unendo il namespace radice, la
/// cartella delle risorse e il nome di questo tipo: spostandolo accanto al file di risorse, il nome cambierebbe e
/// tutti i testi verrebbero mostrati come chiavi.
/// Il traduttore è già disponibile in ogni componente: si usa indicando la chiave, con un valore in più per i testi
/// che contengono un segnaposto. Le convenzioni sui nomi delle chiavi sono scritte in testa al file di risorse.
public sealed class SharedResource
{
}
