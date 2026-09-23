using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// <remarks>Forma di FORM: vedi le tre forme documentate in <c>Features/Documents/Contracts/DocumentResponse.cs</c>.</remarks>
/// <summary>
/// Modello del form di creazione/modifica documento
/// </summary>
/// <remarks>
/// <para>
/// Condiviso da <c>Features/Documents/Views/Pages/DocumentCreate.razor</c> e <c>DocumentEdit.razor</c> tramite il componente <c>DocumentForm</c>.
/// Conversione verso le API: operatori espliciti di <c>Features/Documents/Contracts/DocumentCreateRequest</c> e
/// <c>DocumentUpdateRequest</c>, che aggiungono la valuta EUR.
/// </para>
/// <para>
/// Differenze tra i due casi: in creazione si usa <see cref="Type"/> (la data la imposta il server);
/// in modifica si usa <see cref="Date"/> (il tipo non è modificabile).
/// </para>
/// </remarks>
public class DocumentEditModel
{
    /// <summary>
    /// Tipologia (usata solo in creazione)
    /// </summary>
    public DocumentType Type { get; set; } = DocumentType.Quote;

    /// <summary>
    /// Data documento (usata solo in modifica: in creazione la imposta il server)
    /// </summary>
    /// <remarks>Solo la parte data è significativa: viene inviata come mezzanotte UTC.</remarks>
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public CustomerEditModel Customer { get; set; } = new();

    /// <summary>
    /// Righe del documento: ogni riga è validata da DocumentLineEditModelValidator, richiamato con RuleForEach
    /// da <c>Validators/DocumentEditModelValidator</c>.
    /// </summary>
    public List<DocumentLineEditModel> Lines { get; set; } = new();

    /// <summary>
    /// Anteprima del totale documento (il valore definitivo è calcolato dal server)
    /// </summary>
    public decimal Total => Math.Round(Lines.Sum(line => line.Total), 2);

    /// <summary>
    /// Precompila il form di modifica a partire dal documento letto dalle API: <c>(DocumentEditModel)document</c>.
    /// </summary>
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
