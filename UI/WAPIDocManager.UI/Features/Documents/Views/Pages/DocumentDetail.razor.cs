using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Formatting;
using WAPIDocManager.UI.Shared.Navigation;

namespace WAPIDocManager.UI.Features.Documents.Views.Pages;

/// Codice della pagina di dettaglio del documento: markup e descrizione dei flussi stanno nel file affiancato, e le
/// due parti formano la stessa classe.
/// La logica sta nel ViewModel. Qui restano i ruoli dell'utente, i testi tradotti di titolo ed esito, e la
/// navigazione che segue l'eliminazione e la generazione.
/// Il ridisegno della pagina non va richiesto a mano: è la classe base ad ascoltare i cambiamenti del ViewModel e a
/// provocarlo.
public sealed partial class DocumentDetail
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter] public string Id { get; set; } = string.Empty;

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private IReadOnlyCollection<RoleType> _roles = Array.Empty<RoleType>();

    private string PageTitleText => ViewModel.Document is null
        ? L["Document_Details"]
        : $"{L.Label(ViewModel.Document.Type)} {DisplayFormat.ShortNumber(ViewModel.Document.Number)}";

    // testo dell'avviso di esito: il ViewModel segnala solo che cosa è accaduto, non come dirlo
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

    // passando da un documento all'altro questa istanza viene riusata: è il ViewModel a rileggere solo se
    // l'identificativo è cambiato
    protected override async Task OnParametersSetAsync()
    {
        await ViewModel.LoadAsync(Id);
    }

    private async Task DeleteAsync()
    {
        if (await ViewModel.DeleteAsync())
        {
            Navigation.NavigateTo(AppRoutes.PageRelativeUrls.Documents);
        }
    }

    private async Task GenerateAsync()
    {
        Document? generated = await ViewModel.GenerateAsync();

        if (generated is not null)
        {
            Navigation.NavigateTo(AppRoutes.GetDocumentDetail(generated.Id));
        }
    }
}
