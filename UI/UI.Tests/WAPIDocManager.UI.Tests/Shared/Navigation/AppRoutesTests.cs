using WAPIDocManager.UI.Shared.Navigation;

namespace WAPIDocManager.UI.Tests.Shared.Navigation;

/// Costruzione dei link: gli Id finiscono nell'URL, quindi vanno codificati, e la modifica può portare con sé
/// la pagina di origine per il ritorno.
public class AppRoutesTests
{
    [Fact]
    public void Document_Links_Are_Relative_And_Encode_The_Id()
    {
        // niente slash iniziale: i link sono relativi a <base href>, così l'app regge anche sotto un sotto-percorso
        Assert.Equal("documents/a%2Fb", AppRoutes.GetDocumentDetail("a/b"));
        Assert.Equal("documents/a%2Fb/edit", AppRoutes.GetDocumentEdit("a/b"));
    }

    [Fact]
    public void Edit_Link_Carries_The_Origin_When_Requested()
    {
        Assert.Equal("documents/abc/edit", AppRoutes.GetDocumentEdit("abc"));
        Assert.Equal("documents/abc/edit?returnUrl=documents", AppRoutes.GetDocumentEdit("abc", "documents"));
    }

    [Fact]
    public void Login_Link_Carries_The_Return_Url_When_Present()
    {
        Assert.Equal("login", AppRoutes.LoginWithReturnUrl(null));
        Assert.Equal("login?returnUrl=documents%2Fabc", AppRoutes.LoginWithReturnUrl("documents/abc"));
    }
}
