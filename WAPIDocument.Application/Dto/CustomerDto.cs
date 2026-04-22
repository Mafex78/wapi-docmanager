using WAPIDocument.Domain.Entities.TaxEntities;

namespace WAPIDocument.Application.Dto;

public record CustomerDto()
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }
    
    public static explicit operator CustomerDto(Customer customer)
    {
        return new CustomerDto
        {
            Name = customer.Name,
            Email = customer.Email,
            VatNumber = customer.VatNumber,
            Address = customer.Address
        };
    }
    
    public static explicit operator Customer(CustomerDto model)
    {
        return new Customer
        {
            Name = model.Name,
            Email = model.Email,
            VatNumber = model.VatNumber,
            Address = model.Address
        };
    }
}