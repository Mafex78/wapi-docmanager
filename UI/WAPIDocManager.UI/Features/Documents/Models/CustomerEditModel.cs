using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// <summary>
/// Dati cliente nel form documento.
/// </summary>
/// <remarks>
/// Nessuna validazione, come nel backend in creazione/modifica: Name e VatNumber sono richiesti solo per
/// portare il documento in Ready (<c>DocumentRules.IsComplete</c>). I valori vuoti vengono inviati come null
/// e quelli valorizzati con trim (operatore esplicito di <c>Features/Documents/Contracts/CustomerDto</c>).
/// </remarks>
public class CustomerEditModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }

    /// <summary>
    /// Cliente del documento letto dalle API → dati del form.
    /// </summary>
    public static explicit operator CustomerEditModel(Customer customer)
    {
        return new CustomerEditModel
        {
            Name = customer.Name,
            Email = customer.Email,
            VatNumber = customer.VatNumber,
            Address = customer.Address
        };
    }
}
