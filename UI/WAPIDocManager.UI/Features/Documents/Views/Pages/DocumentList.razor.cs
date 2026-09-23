using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Documents.Views.Pages;

/// <summary>
/// Codice della pagina lista documenti.
/// </summary>
/// <remarks>
/// Markup e descrizione dei flussi in <c>DocumentList.razor</c> (le due parti formano la stessa classe).
/// La logica (ricerca, filtri, paginazione, eliminazione) è in <see cref="DocumentListViewModel"/>, testabile senza Blazor.
/// Qui restano solo i ruoli dell'utente (dallo stato di autenticazione) e i testi localizzati dei tooltip.
/// </remarks>
public partial class DocumentList
{
    // Vm arriva da MvvmComponentBase<DocumentListViewModel>, dichiarata con @inherits nel .razor
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private IReadOnlyCollection<RoleType> _roles = Array.Empty<RoleType>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _roles = (await AuthenticationStateTask).User.GetRoles();
        await Vm.LoadAsync();
    }

    // tooltip del pulsante Modifica disabilitato (il ruolo ha la precedenza sullo stato, vedi DocumentPermissions)
    private string EditDenialMessage(PermissionDenialReason reason, DocumentStatus status)
    {
        return reason == PermissionDenialReason.MissingRole
            ? L["Permission_ReadOnlyRole"]
            : L["Permission_EditInvalidStatus", L.Label(status)];
    }

    // tooltip del pulsante Elimina disabilitato
    private string DeleteDenialMessage(PermissionDenialReason reason, DocumentStatus status)
    {
        return reason == PermissionDenialReason.MissingRole
            ? L["Permission_ReadOnlyRole"]
            : L["Permission_DeleteInvalidStatus", L.Label(status)];
    }
}
