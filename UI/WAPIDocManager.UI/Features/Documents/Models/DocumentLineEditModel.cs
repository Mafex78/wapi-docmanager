using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// Riga di documento mentre la si compila, con le regole allineate a quelle applicate dal server.
/// È una classe mutabile e non un record perché viene legata direttamente ai campi del form, e perché l'istanza
/// stessa fa da identità della riga: serve sia a ridisegnare correttamente l'elenco quando si aggiungono o tolgono
/// righe, sia ad attribuire i messaggi di validazione alla riga giusta.
public class DocumentLineEditModel
{
    public string? Description { get; set; }

    /// Le righe nuove partono da 1, che è la quantità più probabile.
    public decimal Quantity { get; set; } = 1M;
    public decimal UnitPrice { get; set; }

    /// Anteprima del totale di riga mentre si compila, con lo stesso arrotondamento usato dal server; il valore
    /// definitivo lo calcola comunque lui.
    public decimal Total => Math.Round(Quantity * UnitPrice, 2);

    /// Dalla riga letta dalle API alla riga del form. Il totale non viene copiato ma ricalcolato, perché qui è
    /// soltanto un'anteprima.
    public static explicit operator DocumentLineEditModel(DocumentLine line)
    {
        return new DocumentLineEditModel
        {
            Description = line.Description,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice
        };
    }
}
