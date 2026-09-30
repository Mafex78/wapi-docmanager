using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Documents.Views.Pages;

/// Codice della pagina dell'elenco documenti: markup e descrizione dei flussi stanno nel file affiancato, e le due
/// parti formano la stessa classe.
/// Ricerca, filtri, paginazione ed eliminazione stanno nel ViewModel. Qui restano soltanto i ruoli dell'utente, che
/// arrivano dallo stato di autenticazione, e i testi che spiegano perché un pulsante è disattivato.
public partial class DocumentList
{
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private IReadOnlyCollection<RoleType> _roles = Array.Empty<RoleType>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _roles = (await AuthenticationStateTask).User.GetRoles();
        // si passa dal comando e non dal metodo: è l'esecuzione del comando a disattivare ricerca, azzeramento e
        // paginazione mentre il caricamento è in corso
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    // spiegazione mostrata sul pulsante Modifica disattivato; il ruolo mancante ha la precedenza sullo stato
    private string EditDenialMessage(PermissionDenialReason reason, DocumentStatus status)
    {
        return reason == PermissionDenialReason.MissingRole
            ? L["Permission_ReadOnlyRole"]
            : L["Permission_EditInvalidStatus", L.Label(status)];
    }

    // spiegazione mostrata sul pulsante Elimina disattivato
    private string DeleteDenialMessage(PermissionDenialReason reason, DocumentStatus status)
    {
        return reason == PermissionDenialReason.MissingRole
            ? L["Permission_ReadOnlyRole"]
            : L["Permission_DeleteInvalidStatus", L.Label(status)];
    }
}
