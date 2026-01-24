namespace QIMy.Core.Entities;

public class ReturnItem : BaseEntity
{
    public int ReturnId { get; set; }
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? TaxId { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public Return Return { get; set; } = null!;
    public Product? Product { get; set; }
    public Tax? Tax { get; set; }
}
