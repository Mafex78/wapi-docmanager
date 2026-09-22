using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Cliente (<c>WAPIDocument.Application/Dto/CustomerDto.cs</c>), usato sia nei body sia nelle risposte.
/// </summary>
/// <remarks>
/// Copia lato client del DTO del backend (il client non può referenziare WAPIDocument.Application):
/// se cambia sul server va aggiornato qui. JSON camelCase (vedi Http/JsonDefaults).
/// </remarks>
public record CustomerDto
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }

    /// <summary>
    /// Form → DTO: il cliente viene sempre inviato (mai null), con i campi vuoti a null e gli altri con trim.
    /// </summary>
    public static explicit operator CustomerDto(CustomerEditModel customer)
    {
        return new CustomerDto
        {
            Name = DocumentMapper.Normalize(customer.Name),
            Email = DocumentMapper.Normalize(customer.Email),
            VatNumber = DocumentMapper.Normalize(customer.VatNumber),
            Address = DocumentMapper.Normalize(customer.Address)
        };
    }

    /// <summary>
    /// DTO → cliente del client.
    /// </summary>
    public static explicit operator Customer(CustomerDto customer)
    {
        return new Customer
        {
            Name = customer.Name,
            Email = customer.Email,
            VatNumber = customer.VatNumber,
            Address = customer.Address
        };
    }
}
