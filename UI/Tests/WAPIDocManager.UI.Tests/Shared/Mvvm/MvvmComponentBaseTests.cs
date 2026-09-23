using CommunityToolkit.Mvvm.ComponentModel;
using WAPIDocManager.UI.Shared.Mvvm;

namespace WAPIDocManager.UI.Tests.Shared.Mvvm;

/// <summary>
/// Base dei componenti con ViewModel. Il ciclo di vita completo (OnInitialized, ridisegno) richiede un renderer,
/// quindi qui si verifica ciò che è verificabile senza Blazor: che lo smontaggio di un componente mai inizializzato
/// dal renderer non sollevi eccezioni. È lo scenario dei test costruiti a mano, compresi i futuri test con bUnit.
/// </summary>
public class MvvmComponentBaseTests
{
    [Fact]
    public void Dispose_Without_Injected_ViewModel_Does_Not_Throw()
    {
        var component = new TestComponent();

        // due volte: Dispose deve essere idempotente
        Exception? thrown = Record.Exception(() =>
        {
            component.Dispose();
            component.Dispose();
        });

        Assert.Null(thrown);
    }

    private sealed class TestViewModel : ObservableObject
    {
    }

    private sealed class TestComponent : MvvmComponentBase<TestViewModel>
    {
    }
}
