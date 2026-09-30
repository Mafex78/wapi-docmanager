using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Body di PUT api/v1/documents/{id}.
/// Sostituisce per intero data, valuta, cliente e righe: non è un aggiornamento parziale, quindi ciò che non viene
/// inviato viene perso. La tipologia del documento non è modificabile e infatti non compare.
public record DocumentUpdateRequest
{
    /// Inviata come mezzanotte UTC; il server ne considera solo anno, mese e giorno.
    public DateTime Date { get; init; }

    /// Va sempre valorizzata: lasciandola vuota il server azzererebbe la valuta del documento.
    public string? Currency { get; init; }

    public CustomerDto? Customer { get; init; }
    public IList<DocumentLineRequest> DocumentLines { get; init; } = new List<DocumentLineRequest>();

    /// Dal form al corpo della modifica.
    public static explicit operator DocumentUpdateRequest(DocumentEditModel model)
    {
        return new DocumentUpdateRequest
        {
            // la data viene normalizzata a mezzanotte UTC, come la conserva il server
            Date = DateTime.SpecifyKind(model.Date.Date, DateTimeKind.Utc),
            Currency = DocumentCurrency.Code,
            Customer = (CustomerDto)model.Customer,
            DocumentLines = model.Lines
                .Select(line => (DocumentLineRequest)line)
                .ToList()
        };
    }
}
