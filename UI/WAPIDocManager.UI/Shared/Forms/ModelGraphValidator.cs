using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Shared.Forms;

/// <summary>
/// Validatore dell'EditForm basato su <see cref="ModelValidator"/>: valida l'intero grafo del modello
/// (es. le righe documento) e registra come messaggi le chiavi di localizzazione.
/// </summary>
/// <remarks>
/// <para>
/// Da usare dentro ogni EditForm al posto di <c>DataAnnotationsValidator</c>, che non valida oggetti annidati e collezioni.
/// I messaggi salvati nell'EditContext sono chiavi di <c>ValidationKeys</c>: vanno mostrati con <see cref="FieldValidationMessage{TValue}"/>
/// (che li traduce) e non con il ValidationMessage standard (mostrerebbe la chiave).
/// </para>
/// <para>
/// Submit: OnValidationRequested rivaluta tutto il grafo, quindi OnValidSubmit parte solo se non ci sono errori.
/// Modifica di un campo: si rivaluta il grafo ma si aggiornano solo i messaggi di quel campo,
/// per non mostrare errori su campi che l'utente non ha ancora toccato.
/// </para>
/// <para>
/// Gli input di Blazor ricevono la classe CSS "invalid" (non "is-invalid" di Bootstrap): lo stile è in app.css.
/// </para>
/// </remarks>
public sealed class ModelGraphValidator : ComponentBase, IDisposable
{
    private EditContext? _editContext;
    private ValidationMessageStore? _messages;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    protected override void OnParametersSet()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(ModelGraphValidator)} requires a cascading {nameof(EditContext)}.");
        }

        if (ReferenceEquals(CurrentEditContext, _editContext))
        {
            return;
        }

        Detach();

        _editContext = CurrentEditContext;
        _messages = new ValidationMessageStore(_editContext);
        _editContext.OnValidationRequested += OnValidationRequested;
        _editContext.OnFieldChanged += OnFieldChanged;
    }

    public void Dispose()
    {
        Detach();
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        _messages!.Clear();

        foreach (ValidationFailure failure in ModelValidator.Validate(_editContext!.Model))
        {
            _messages.Add(new FieldIdentifier(failure.Instance, failure.MemberName), failure.MessageKey);
        }

        _editContext.NotifyValidationStateChanged();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        FieldIdentifier field = args.FieldIdentifier;

        _messages!.Clear(field);

        IEnumerable<ValidationFailure> fieldFailures = ModelValidator.Validate(_editContext!.Model)
            .Where(failure => ReferenceEquals(failure.Instance, field.Model) &&
                              failure.MemberName == field.FieldName);

        foreach (ValidationFailure failure in fieldFailures)
        {
            _messages.Add(field, failure.MessageKey);
        }

        _editContext.NotifyValidationStateChanged();
    }

    private void Detach()
    {
        if (_editContext is null)
        {
            return;
        }

        _editContext.OnValidationRequested -= OnValidationRequested;
        _editContext.OnFieldChanged -= OnFieldChanged;
        _messages?.Clear();
        _editContext = null;
        _messages = null;
    }
}
