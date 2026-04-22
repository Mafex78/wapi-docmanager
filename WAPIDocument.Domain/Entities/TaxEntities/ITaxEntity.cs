namespace WAPIDocument.Domain.Entities.TaxEntities;

public interface ITaxEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }
}