using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WAPIDocManager.UI.Shared.Navigation;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Shared.Navigation;

/// <summary>
/// Comportamento deciso per l'indirizzo di ritorno: assente è il caso normale e non va segnalato, valido si usa,
/// presente ma rifiutato ripiega E lascia un avviso nella console del browser — senza il quale un link generato
/// male dall'applicazione sarebbe indistinguibile da uno confezionato da terzi.
/// </summary>
public class ReturnUrlResolverTests
{
    private const string Fallback = "documents";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_Value_Falls_Back_Without_Logging(string? candidate)
    {
        var logger = new RecordingLogger<ReturnUrlResolver>();
        var resolver = new ReturnUrlResolver(logger);

        Assert.Equal(Fallback, resolver.Resolve(candidate, Fallback));
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public void Valid_Value_Is_Used_Without_Logging()
    {
        var logger = new RecordingLogger<ReturnUrlResolver>();
        var resolver = new ReturnUrlResolver(logger);

        Assert.Equal("documents/abc", resolver.Resolve("documents/abc", Fallback));
        Assert.Empty(logger.Entries);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("//evil.example")]
    [InlineData("login")]
    public void Rejected_Value_Falls_Back_And_Warns(string candidate)
    {
        var logger = new RecordingLogger<ReturnUrlResolver>();
        var resolver = new ReturnUrlResolver(logger);

        Assert.Equal(Fallback, resolver.Resolve(candidate, Fallback, "login"));

        (LogLevel Level, string Message) entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains(candidate, entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolver_Works_With_The_Null_Logger()
    {
        // il resolver non deve dipendere dalla presenza di un logger reale
        var resolver = new ReturnUrlResolver(NullLogger<ReturnUrlResolver>.Instance);

        Assert.Equal(Fallback, resolver.Resolve("https://evil.example", Fallback));
    }
}
