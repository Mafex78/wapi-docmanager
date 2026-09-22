using Microsoft.JSInterop;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Tests.TestSupport;

/// <summary>
/// IJSRuntime in memoria che simula sessionStorage
/// </summary>
/// <remarks>
/// Supporta solo gli identificatori usati da SessionStorageUserSessionStore (getItem, setItem, removeItem);
/// InvokeVoidAsync passa da InvokeAsync, quindi è coperto. Un identificatore diverso solleva NotSupportedException.
/// </remarks>
public sealed class FakeJsRuntime : IJSRuntime
{
    public Dictionary<string, string> SessionStorage { get; } = new();

    public int GetItemCalls { get; private set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        string key = (string)args![0]!;

        switch (identifier)
        {
            case "sessionStorage.getItem":
                GetItemCalls++;
                SessionStorage.TryGetValue(key, out string? value);
                return ValueTask.FromResult((TValue)(object?)value!);

            case "sessionStorage.setItem":
                SessionStorage[key] = (string)args[1]!;
                return ValueTask.FromResult(default(TValue)!);

            case "sessionStorage.removeItem":
                SessionStorage.Remove(key);
                return ValueTask.FromResult(default(TValue)!);

            default:
                throw new NotSupportedException(identifier);
        }
    }
}
