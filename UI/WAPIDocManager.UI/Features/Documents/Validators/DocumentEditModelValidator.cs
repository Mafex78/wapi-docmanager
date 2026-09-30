using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// Regole del form di creazione e modifica: il documento in sé non ne ha, perché data e tipologia sono sempre
/// valorizzate dal form e il totale è calcolato. Si limita quindi a delegare alle regole del cliente e a quelle
/// delle singole righe.
/// È la libreria di validazione a percorrere l'oggetto annidato e la collezione, e gli errori arrivano al form già
/// attribuiti al campo giusto, riga per riga: è la ragione per cui i messaggi compaiono accanto all'input
/// sbagliato e non in fondo alla pagina.
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
