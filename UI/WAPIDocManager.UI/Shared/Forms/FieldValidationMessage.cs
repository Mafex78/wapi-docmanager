using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using WAPIDocManager.UI;

namespace WAPIDocManager.UI.Shared.Forms;

/// <summary>
/// Mostra i messaggi di validazione di un campo traducendo le chiavi con IStringLocalizer
/// </summary>
/// <remarks>
/// <para>
/// Sostituisce il ValidationMessage standard nei form che usano <see cref="ModelGraphValidator"/>.
/// Uso: <c>&lt;FieldValidationMessage For="() =&gt; line.Quantity" /&gt;</c>; funziona anche per elementi di collezioni
/// perché il FieldIdentifier è creato dall'espressione (istanza della riga + nome proprietà).
/// </para>
/// <para>
/// Componente scritto in C# (non .razor): gli @inject di _Imports.razor non valgono qui, per questo il localizzatore è iniettato con [Inject].
/// </para>
/// </remarks>
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
