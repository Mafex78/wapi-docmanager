using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Formatting;
using WAPIDocManager.UI.Shared.Navigation;

namespace WAPIDocManager.UI.Features.Documents.Views.Pages;

/// <summary>
/// Codice della pagina di dettaglio documento.
/// </summary>
/// <remarks>
/// Markup e descrizione dei flussi in <c>DocumentDetail.razor</c> (le due parti formano la stessa classe).
/// La logica è in <see cref="DocumentDetailViewModel"/>, testabile senza Blazor. Qui restano: ruoli dell'utente,
/// testi localizzati (titolo e messaggio di esito), navigazione dopo eliminazione e generazione, e il ridisegno
/// richiesto dal ViewModel durante il caricamento dei documenti collegati.
/// </remarks>
// sealed: il componente non viene ereditato e senza finalizzatore basta un Dispose() semplice
// (soddisfa CA1816/S3881 senza il cerimoniale del dispose pattern, che qui non serve)
public sealed partial class DocumentDetail : IDisposable
{
    [Inject] private DocumentDetailViewModel Vm { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter] public string Id { get; set; } = string.Empty;

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private IReadOnlyCollection<RoleType> _roles = Array.Empty<RoleType>();

    private string PageTitleText => Vm.Document is null
        ? L["Document_Details"]
        : $"{L.Label(Vm.Document.Type)} {DisplayFormat.ShortNumber(Vm.Document.Number)}";

    // testo dell'avviso verde: il ViewModel segnala solo cosa è accaduto (DocumentDetailNotification)
    private string? NotificationMessage => Vm.Notification switch
    {
        DocumentDetailNotification.StatusUpdated => L["Detail_StatusUpdated", L.Label(Vm.NotificationStatus!.Value)].Value,
        DocumentDetailNotification.Attached => L["Detail_Attached"].Value,
        _ => null
    };

    protected override async Task OnInitializedAsync()
    {
        // il ViewModel chiede un ridisegno quando lo stato cambia fuori dai gestori di evento
        Vm.StateChanged += OnViewModelStateChanged;

        _roles = (await AuthenticationStateTask).User.GetRoles();
    }

    // navigare da un documento all'altro riusa questa istanza: il ViewModel ricarica solo se l'Id è cambiato
    protected override async Task OnParametersSetAsync()
    {
        await Vm.LoadAsync(Id);
    }

    public void Dispose()
    {
        Vm.StateChanged -= OnViewModelStateChanged;
    }

    private void OnViewModelStateChanged()
    {
        StateHasChanged();
    }

    private async Task DeleteAsync()
    {
        if (await Vm.DeleteAsync())
        {
            Navigation.NavigateTo(AppRoutes.Documents);
        }
    }

    private async Task GenerateAsync()
    {
        Document? generated = await Vm.GenerateAsync();

        if (generated is not null)
        {
            Navigation.NavigateTo(AppRoutes.DocumentDetail(generated.Id));
        }
    }
}
