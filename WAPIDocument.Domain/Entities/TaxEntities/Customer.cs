namespace WAPIDocument.Domain.Entities.TaxEntities;

public class Customer : ITaxEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }

    public Customer Clone()
    {
        return new Customer
        {
            Name = Name,
            Email = Email,
            VatNumber = VatNumber,
            Address = Address,
        };
    }
}