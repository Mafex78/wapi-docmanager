using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// Dati del cliente dentro il form del documento.
/// Non hanno regole di validazione, come sul server: ragione sociale e partita IVA diventano obbligatorie solo per
/// far avanzare il documento oltre la bozza, non per salvarlo. Inviando, i campi vuoti diventano null e quelli
/// compilati vengono ripuliti dagli spazi.
public class CustomerEditModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }

    /// Dal cliente letto dalle API ai dati del form.
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
