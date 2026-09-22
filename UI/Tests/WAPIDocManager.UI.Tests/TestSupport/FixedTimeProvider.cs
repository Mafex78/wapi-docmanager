namespace WAPIDocManager.UI.Tests.TestSupport;

/// <summary>
/// TimeProvider con ora fissa (UTC) per verificare in modo deterministico la scadenza del token.
/// </summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTime utcNow)
    {
        _utcNow = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}
