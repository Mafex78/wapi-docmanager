using System.Text;
using WAPIDocManager.UI.Features.Auth.Services;

namespace WAPIDocManager.UI.Tests.TestSupport;

/// <summary>
/// Crea token JWT (non firmati) con il payload indicato
/// </summary>
/// <remarks>
/// Sufficiente perché il client legge solo il payload (JwtPayloadReader) e non verifica la firma.
/// </remarks>
public static class JwtTokenFactory
{
    public static string Create(string payloadJson)
    {
        return $"{Encode("""{"alg":"HS256","typ":"JWT"}""")}.{Encode(payloadJson)}.signature";
    }

    private static string Encode(string json)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
