using WAPIDocManager.UI.Shared.Navigation;

namespace WAPIDocManager.UI.Tests.Shared.Navigation;

/// <summary>
/// Validazione dell'indirizzo di ritorno letto dalla query string: è la protezione contro l'open redirect.
/// Qui si verifica solo l'esito della validazione; cosa farne (ripiego e avviso) è di ReturnUrlResolver.
/// </summary>
public class SafeReturnUrlTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    // assoluti travestiti da relativi: il browser li tratterebbe come host esterni
    [InlineData("/documents")]
    [InlineData("//evil.example")]
    [InlineData("\\evil.example")]
    [InlineData("https://evil.example")]
    public void Unusable_Values_Are_Rejected(string? candidate)
    {
        Assert.False(SafeReturnUrl.TryGetSafeRelativeUrl(candidate, out string? safeUrl));
        Assert.Null(safeUrl);
    }

    [Theory]
    [InlineData("documents")]
    [InlineData("documents/abc")]
    [InlineData("documents?page=2")]
    // i segmenti ".." sono ammessi da Uri.IsWellFormedUriString e restano innocui: risolti contro <base href>
    // portano comunque dentro l'applicazione, al massimo su una rotta inesistente
    [InlineData("documents/../other")]
    public void Relative_Paths_Are_Accepted(string candidate)
    {
        Assert.True(SafeReturnUrl.TryGetSafeRelativeUrl(candidate, out string? safeUrl));
        Assert.Equal(candidate, safeUrl);
    }

    [Theory]
    [InlineData("login")]
    [InlineData("LOGIN?returnUrl=documents")]
    public void Forbidden_Prefixes_Are_Rejected(string candidate)
    {
        // il login rifiuta il ritorno a se stesso per non creare un ciclo
        Assert.False(SafeReturnUrl.TryGetSafeRelativeUrl(candidate, out _, "login"));
    }
}
