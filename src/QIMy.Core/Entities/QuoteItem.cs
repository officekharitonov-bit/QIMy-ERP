namespace QIMy.Core.Entities;

public class QuoteItem : BaseEntity
{
    public int QuoteId { get; set; }
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? TaxId { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public Quote Quote { get; set; } = null!;
    public Product? Product { get; set; }
    public Tax? Tax { get; set; }
}
