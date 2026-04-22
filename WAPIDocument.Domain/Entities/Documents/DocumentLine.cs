namespace WAPIDocument.Domain.Entities.Documents;

public class DocumentLine
{
    public string? Description { get; set; } = string.Empty;
    
    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            Recalculate();
        }
    }
    
    private decimal _unitPrice;
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            _unitPrice = value;
            Recalculate();
        }
    }
    
    public decimal Total { get; private set; }

    private void Recalculate()
    {
        Total = Math.Round(_quantity * _unitPrice, 2);
    }

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Description) ||
            (Quantity <= 0M || UnitPrice <= 0M))
        {
            return false;
        }

        return true;
    }

    public DocumentLine Clone()
    {
        return new DocumentLine
        {
            Description = Description,
            Quantity = Quantity,
            UnitPrice = UnitPrice,
        };
    }
}