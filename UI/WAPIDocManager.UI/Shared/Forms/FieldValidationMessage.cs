using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using WAPIDocManager.UI;

namespace WAPIDocManager.UI.Shared.Forms;

/// Mostra i messaggi di validazione di un campo traducendoli.
/// Sostituisce il componente standard perché le regole dei form non scrivono un testo ma una CHIAVE di risorsa: la
/// traduzione va fatta qui, nel momento in cui il messaggio compare.
/// Funziona anche per gli elementi di una collezione, come le righe di un documento, perché il campo da osservare
/// viene ricavato dall'espressione passata: da lì arrivano sia l'oggetto sia il nome della proprietà.
/// È scritto in C# e non come componente di markup: per questo il traduttore va iniettato esplicitamente, dato che
/// le iniezioni globali dei componenti di markup qui non valgono.
public sealed class FieldValidationMessage<TValue> : ComponentBase, IDisposable
{
    private EditContext? _editContext;
    private FieldIdentifier _field;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Inject]
    private IStringLocalizer<SharedResource> Localizer { get; set; } = default!;

    [Parameter, EditorRequired]
    public Expression<Func<TValue>>? For { get; set; }

    protected override void OnParametersSet()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(FieldValidationMessage<TValue>)} requires a cascading {nameof(EditContext)}.");
        }

        if (For is null)
        {
            throw new InvalidOperationException($"{nameof(FieldValidationMessage<TValue>)} requires the {nameof(For)} parameter.");
        }

        _field = FieldIdentifier.Create(For);

        if (!ReferenceEquals(CurrentEditContext, _editContext))
        {
            Detach();
            _editContext = CurrentEditContext;
            _editContext.OnValidationStateChanged += OnValidationStateChanged;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_editContext is null)
        {
            return;
        }

        foreach (string messageKey in _editContext.GetValidationMessages(_field))
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "invalid-feedback d-block");
            builder.AddContent(2, Localizer[messageKey].Value);
            builder.CloseElement();
        }
    }

    public void Dispose()
    {
        Detach();
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs args)
    {
        StateHasChanged();
    }

    private void Detach()
    {
        if (_editContext is not null)
        {
            _editContext.OnValidationStateChanged -= OnValidationStateChanged;
            _editContext = null;
        }
    }
}
