using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Cliente, nella forma che viaggia sia nelle richieste sia nelle risposte.
public record CustomerDto
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }

    /// Dal form alla richiesta: il cliente viene sempre inviato, mai omesso, con i campi vuoti azzerati e gli altri
    /// ripuliti dagli spazi.
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

    /// Dalla risposta al cliente dell'applicazione.
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
