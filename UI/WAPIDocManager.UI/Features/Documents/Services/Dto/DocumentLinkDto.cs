using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Collegamento verso un altro documento, come arriva nelle risposte.
/// Porta solo l'identificativo del documento collegato: non dice se il collegamento è stato creato a mano o dalla
/// generazione, né come si chiama quel documento.
public record DocumentLinkDto
{
    public string TargetDocumentId { get; init; } = string.Empty;

    /// Dalla risposta al collegamento dell'applicazione.
    public static explicit operator DocumentLink(DocumentLinkDto link)
    {
        return new DocumentLink
        {
            TargetDocumentId = link.TargetDocumentId
        };
    }
}
