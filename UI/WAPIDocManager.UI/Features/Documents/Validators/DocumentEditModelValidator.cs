using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// <summary>
/// Regole del form di creazione/modifica documento: delega ai validator del cliente e delle righe
/// </summary>
/// <remarks>
/// <para>
/// <c>SetValidator</c> e <c>RuleForEach</c> sostituiscono l'attributo NestedValidation e il motore di validazione
/// del grafo scritti a mano: è FluentValidation a percorrere l'oggetto annidato e la collezione, e gli errori
/// arrivano all'EditContext con il percorso corretto (es. <c>Lines[0].Quantity</c>).
/// </para>
/// <para>
/// Il documento non ha regole proprie: data e tipologia sono sempre valorizzate dal form, il totale è calcolato.
/// </para>
/// </remarks>
public sealed class DocumentEditModelValidator : AbstractValidator<DocumentEditModel>
{
    public DocumentEditModelValidator()
    {
        RuleFor(model => model.Customer)
            .SetValidator(new CustomerEditModelValidator());

        RuleForEach(model => model.Lines)
            .SetValidator(new DocumentLineEditModelValidator());
    }
}
