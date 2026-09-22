using System.Text.Json;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Stesse impostazioni JSON di ASP.NET Core (camelCase, case-insensitive, enum numerici)
/// </summary>
/// <remarks>
/// Usate per tutte le chiamate HTTP e per serializzare <c>UserSession</c> nel sessionStorage.
/// Le API non registrano JsonStringEnumConverter: aggiungerlo qui (o sul server) rompe la compatibilità.
/// </remarks>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
