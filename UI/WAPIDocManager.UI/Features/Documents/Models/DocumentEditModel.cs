using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// Modello del form di creazione e modifica del documento, condiviso dalle due pagine attraverso lo stesso
/// componente di form.
/// Un solo modello per due casi d'uso che differiscono in due punti: in creazione si sceglie la tipologia, e la data
/// la decide il server; in modifica si cambia la data, e la tipologia non è più modificabile.
public class DocumentEditModel
{
    /// Tipologia, usata solo in creazione.
    public DocumentType Type { get; set; } = DocumentType.Quote;

    /// Data del documento, usata solo in modifica. Conta solo la parte di data: viene inviata come mezzanotte UTC.
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public CustomerEditModel Customer { get; set; } = new();

    /// Righe del documento, ciascuna validata singolarmente dalle regole del form.
    public List<DocumentLineEditModel> Lines { get; set; } = new();

    /// Anteprima del totale mentre si compila: il valore definitivo lo calcola comunque il server.
    public decimal Total => Math.Round(Lines.Sum(line => line.Total), 2);

    /// Precompila il form a partire dal documento letto dalle API.
    public static explicit operator DocumentEditModel(Document document)
    {
        return new DocumentEditModel
        {
            Type = document.Type,
            Date = document.Date.Date,
            Customer = (CustomerEditModel)document.Customer,
            Lines = document.Lines
                .Select(line => (DocumentLineEditModel)line)
                .ToList()
        };
    }
}
