using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace WAPIDocManager.UI.Shared.Mvvm;

/// <summary>
/// Base delle pagine che hanno un ViewModel: lo inietta e ridisegna il componente quando il ViewModel notifica
/// un cambiamento di stato.
/// </summary>
/// <remarks>
/// <para>
/// USO: nel file .razor va dichiarata la base, non nel code-behind —
/// <c>@inherits MvvmComponentBase&lt;DocumentListViewModel&gt;</c> — e la partial .razor.cs resta
/// <c>public partial class DocumentList</c> senza base, altrimenti le due dichiarazioni parziali avrebbero
/// basi diverse (errore CS0263). Il ViewModel è disponibile come <see cref="Vm"/>: niente [Inject] nella pagina.
/// </para>
/// <para>
/// PERCHÉ ESISTE: Blazor ridisegna da solo dopo i gestori di evento, ma non quando lo stato cambia altrove
/// (es. un caricamento asincrono che prosegue dopo il click). Prima questa esigenza era coperta da un evento
/// <c>StateChanged</c> scritto a mano nel ViewModel; ora arriva da <see cref="INotifyPropertyChanged"/> di
/// <c>ObservableObject</c> (CommunityToolkit.Mvvm), che i ViewModel ottengono gratis.
/// </para>
/// <para>
/// PERCHÉ NON Blazing.Mvvm: farebbe esattamente questo, ma mettendo un pacchetto di terze parti nella classe base
/// di tutte le pagine — la dipendenza più costosa da rimuovere. Qui la parte Blazor sono queste righe; il resto
/// del pattern (ObservableObject, [ObservableProperty], [RelayCommand], messenger) è pacchetto Microsoft.
/// </para>
/// <para>
/// <see cref="InvokeAsync"/> è necessario perché la notifica può arrivare da un thread diverso da quello del
/// renderer, e il gestore ignora le notifiche che arrivano dopo lo smontaggio del componente.
/// Una pagina che deve liberare risorse proprie sovrascrive <see cref="Dispose(bool)"/>.
/// </para>
/// </remarks>
/// <typeparam name="TViewModel">ViewModel della pagina, registrato Transient nella registrazione dello slice.</typeparam>
public abstract class MvvmComponentBase<TViewModel> : ComponentBase, IDisposable
    where TViewModel : ObservableObject
{
    private bool _disposed;

    [Inject]
    protected TViewModel Vm { get; set; } = default!;

    /// <summary>
    /// Iscrive il componente alle notifiche del ViewModel.
    /// </summary>
    /// <remarks>
    /// <c>sealed</c> di proposito: una pagina che lo sovrascrivesse dimenticando <c>base.OnInitialized()</c>
    /// resterebbe senza sottoscrizione e smetterebbe di ridisegnarsi, con un difetto silenzioso e difficile da
    /// diagnosticare. Chi deve inizializzare usa <c>OnInitializedAsync</c>, che ComponentBase invoca subito dopo.
    /// La chiamata a base qui sotto serve nel caso in cui l'implementazione di ComponentBase, oggi vuota, in futuro
    /// contenga del codice.
    /// </remarks>
    protected sealed override void OnInitialized()
    {
        base.OnInitialized();

        Vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disiscrive il componente dal ViewModel. Una pagina che deve liberare risorse proprie sovrascrive questo
    /// metodo e chiama <c>base.Dispose(disposing)</c>: è il dispose pattern classico, richiesto perché questa
    /// classe è pensata per essere ereditata (nessuna risorsa non gestita, quindi niente finalizzatore).
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _disposed = true;

        // in esecuzione il renderer inietta il ViewModel prima di OnInitialized, quindi Vm è sempre valorizzato;
        // il controllo serve quando il componente è costruito a mano, come nei test
        if (Vm is not null)
        {
            Vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // una notifica può arrivare dopo lo smontaggio del componente (un caricamento asincrono che termina
        // quando l'utente ha già navigato altrove): in quel caso non si chiede più un render
        if (_disposed)
        {
            return;
        }

        InvokeAsync(StateHasChanged);
    }
}
