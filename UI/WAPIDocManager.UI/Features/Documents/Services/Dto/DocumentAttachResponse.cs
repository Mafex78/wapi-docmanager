using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Risposta di POST api/v1/documents/{id}/attachments: i collegamenti aggiornati del documento indicato
/// nell'indirizzo.
public record DocumentAttachResponse
{
    public IList<DocumentLinkDto>? LinkedDocuments { get; init; }

    /// Dalla risposta all'elenco dei collegamenti; un elenco assente diventa vuoto.
    /// La conversione produce una lista concreta e non un elenco in sola lettura perché il linguaggio non ammette
    /// operatori di conversione verso un'interfaccia; è poi il servizio a restituirla in sola lettura.
    public static explicit operator List<DocumentLink>(DocumentAttachResponse response)
    {
        return response.LinkedDocuments?
            .Select(link => (DocumentLink)link)
            .ToList() ?? new List<DocumentLink>();
    }
}
