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
public sealed partial class DocumentDetail
{
    // ViewModel arriva da Blazing.Mvvm.Components.MvvmComponentBase<DocumentDetailViewModel>, dichiarata con
    // @inherits nel .razor: è la base a iscriversi a PropertyChanged e a chiamare StateHasChanged, mentre le
    // notifiche dei comandi (IsRunning) arrivano dal ViewModelBase di Blazing, che le rilancia.
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter] public string Id { get; set; } = string.Empty;

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private IReadOnlyCollection<RoleType> _roles = Array.Empty<RoleType>();

    private string PageTitleText => ViewModel.Document is null
        ? L["Document_Details"]
        : $"{L.Label(ViewModel.Document.Type)} {DisplayFormat.ShortNumber(ViewModel.Document.Number)}";

    // testo dell'avviso verde: il ViewModel segnala solo cosa è accaduto (DocumentDetailNotification)
    private string? NotificationMessage => ViewModel.Notification switch
    {
        DocumentDetailNotification.StatusUpdated => L["Detail_StatusUpdated", L.Label(ViewModel.NotificationStatus!.Value)].Value,
        DocumentDetailNotification.Attached => L["Detail_Attached"].Value,
        _ => null
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _roles = (await AuthenticationStateTask).User.GetRoles();
    }

    // navigare da un documento all'altro riusa questa istanza: il ViewModel ricarica solo se l'Id è cambiato
    protected override async Task OnParametersSetAsync()
    {
        await ViewModel.LoadAsync(Id);
    }

    private async Task DeleteAsync()
    {
        if (await ViewModel.DeleteAsync())
        {
            Navigation.NavigateTo(AppRoutes.Documents);
        }
    }

    private async Task GenerateAsync()
    {
        Document? generated = await ViewModel.GenerateAsync();

        if (generated is not null)
        {
            Navigation.NavigateTo(AppRoutes.DocumentDetail(generated.Id));
        }
    }
}
