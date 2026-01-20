namespace QIMy.Core.Entities;

/// <summary>
/// Invoice line item
/// UnitPrice = NETTO (without VAT)
/// TotalAmount = BRUTTO (with VAT)
/// </summary>
public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Unit price (NETTO - without VAT)
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Tax configuration (combines TaxRate + Account/Erlöskonto)
    /// </summary>
    public int? TaxId { get; set; }
    
    /// <summary>
    /// Calculated tax amount
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Total amount including tax (BRUTTO)
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    public int? DiscountId { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public Product? Product { get; set; }
    public Tax? Tax { get; set; }
    public Discount? Discount { get; set; }
}
